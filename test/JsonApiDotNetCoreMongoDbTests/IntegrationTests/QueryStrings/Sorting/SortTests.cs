using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Sorting;

public sealed class SortTests : IClassFixture<IntegrationTestContext<TestableStartup, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SortTests(IntegrationTestContext<TestableStartup, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogPostsController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<WebAccountsController>();
    }

    [Fact]
    public async Task Can_sort_in_primary_resources()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.Generate(3);
        posts[0].Caption = "B";
        posts[1].Caption = "A";
        posts[2].Caption = "C";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?sort=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(posts[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(posts[2].StringId);
    }

    [Fact]
    public async Task Cannot_sort_on_OneToMany_relationship()
    {
        // Arrange
        const string route = "/blogs?sort=count(posts)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_sort_on_ManyToMany_relationship()
    {
        // Arrange
        const string route = "/blogPosts?sort=-count(labels)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_sort_on_ManyToOne_relationship()
    {
        // Arrange
        const string route = "/blogPosts?sort=-author.displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_sort_descending_by_ID()
    {
        // Arrange
        List<WebAccount> accounts = _fakers.WebAccount.Generate(3);
        accounts[0].Id = "5ff752c4f7c9a9a8373991b2";
        accounts[1].Id = "5ff752c3f7c9a9a8373991b1";
        accounts[2].Id = "5ff752c2f7c9a9a8373991b0";

        accounts[0].DisplayName = "B";
        accounts[1].DisplayName = "A";
        accounts[2].DisplayName = "A";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WebAccount>();
            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/webAccounts?sort=displayName,-id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(accounts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(accounts[2].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(accounts[0].StringId);
    }

    [Fact]
    public async Task Sorts_by_ID_if_none_specified()
    {
        // Arrange
        List<WebAccount> accounts = _fakers.WebAccount.Generate(4);
        accounts[0].Id = "5ff8a7bcb2a9b83724282718";
        accounts[1].Id = "5ff8a7bcb2a9b83724282717";
        accounts[2].Id = "5ff8a7bbb2a9b83724282716";
        accounts[3].Id = "5ff8a7bdb2a9b83724282719";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WebAccount>();
            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/webAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(4);
        responseDocument.Data.ManyValue[0].Id.Should().Be(accounts[2].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(accounts[1].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(accounts[0].StringId);
        responseDocument.Data.ManyValue[3].Id.Should().Be(accounts[3].StringId);
    }
}
