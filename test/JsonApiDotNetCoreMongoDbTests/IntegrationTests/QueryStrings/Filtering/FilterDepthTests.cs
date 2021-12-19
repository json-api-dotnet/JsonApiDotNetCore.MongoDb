using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterDepthTests : IClassFixture<IntegrationTestContext<TestableStartup, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public FilterDepthTests(IntegrationTestContext<TestableStartup, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();
        testContext.UseController<BlogPostsController>();
    }

    [Fact]
    public async Task Can_filter_in_primary_resources()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.Generate(2);
        posts[0].Caption = "One";
        posts[1].Caption = "Two";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=equals(caption,'Two')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Cannot_filter_on_ManyToOne_relationship()
    {
        // Arrange
        const string route = "/blogPosts?filter=or(equals(author.userName,'Smith'),equals(author,null))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_filter_on_OneToMany_relationship()
    {
        // Arrange
        const string route = "/blogs?filter=greaterThan(count(posts),'0')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_filter_on_ManyToMany_relationship()
    {
        // Arrange
        const string route = "/blogPosts?filter=has(labels)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }
}
