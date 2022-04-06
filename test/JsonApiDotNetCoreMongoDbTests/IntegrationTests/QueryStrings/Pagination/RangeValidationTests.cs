using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Pagination;

public sealed class RangeValidationTests : IClassFixture<IntegrationTestContext<TestableStartup, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public RangeValidationTests(IntegrationTestContext<TestableStartup, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();
    }

    [Fact]
    public async Task Returns_empty_set_of_resources_when_page_number_is_too_high()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.Generate(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?sort=id&page[size]=3&page[number]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_use_zero_page_size()
    {
        // Arrange
        const string route = "/blogs?page[size]=0";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Can_use_positive_page_size()
    {
        // Arrange
        const string route = "/blogs?page[size]=50";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
