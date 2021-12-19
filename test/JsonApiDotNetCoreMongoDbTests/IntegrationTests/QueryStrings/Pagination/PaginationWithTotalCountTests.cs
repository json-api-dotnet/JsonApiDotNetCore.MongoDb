using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Pagination;

public sealed class PaginationWithTotalCountTests : IClassFixture<IntegrationTestContext<TestableStartup, QueryStringDbContext>>
{
    private const string HostPrefix = "http://localhost";

    private readonly IntegrationTestContext<TestableStartup, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public PaginationWithTotalCountTests(IntegrationTestContext<TestableStartup, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogPostsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;
    }

    [Fact]
    public async Task Can_paginate_in_primary_resources()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?page[number]=2&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/blogPosts?page%5Bnumber%5D=2&page%5Bsize%5D=1");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Uses_default_page_number_and_size()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = new PageSize(2);

        List<BlogPost> posts = _fakers.BlogPost.Generate(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[0].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(posts[1].StringId);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be($"{HostPrefix}{route}?page%5Bnumber%5D=2");
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Returns_all_resources_when_paging_is_disabled()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = null;

        List<BlogPost> posts = _fakers.BlogPost.Generate(25);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(25);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
    }
}
