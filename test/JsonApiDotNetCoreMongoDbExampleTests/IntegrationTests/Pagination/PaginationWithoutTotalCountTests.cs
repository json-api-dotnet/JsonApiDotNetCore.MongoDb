using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Pagination
{
    public sealed class PaginationWithoutTotalCountTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private const int _defaultPageSize = 5;
        private readonly IntegrationTestContext<Startup> _testContext;
        private readonly Faker<Article> _articleFaker = new Faker<Article>();

        public PaginationWithoutTotalCountTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            
            options.IncludeTotalResourceCount = false;
            options.DefaultPageSize = new PageSize(_defaultPageSize);
            options.AllowUnknownQueryStringParameters = true;
        }

        [Fact]
        public async Task When_page_size_is_unconstrained_it_should_not_render_pagination_links()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = null;

            var route = "/api/v1/articles?foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task When_page_size_is_specified_in_query_string_with_no_data_it_should_render_pagination_links()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = null;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Article>(nameof(Article)).DeleteManyAsync(Builders<Article>.Filter.Empty);
            });

            var route = "/api/v1/articles?page[size]=8&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?page[size]=8&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?page[size]=8&foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task When_page_number_is_specified_in_query_string_with_no_data_it_should_render_pagination_links()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Article>(nameof(Article)).DeleteManyAsync(Builders<Article>.Filter.Empty);
            });

            var route = "/api/v1/articles?page[number]=2&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?page[number]=2&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task When_page_number_is_specified_in_query_string_with_partially_filled_page_it_should_render_pagination_links()
        {
            // Arrange
            var articles = _articleFaker.Generate(12);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Article>(nameof(Article));
                await collection.DeleteManyAsync(Builders<Article>.Filter.Empty);
                await collection.InsertManyAsync(articles);
            });

            var route = "/api/v1/articles?foo=bar&page[number]=3";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Count.Should().BeLessThan(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?foo=bar&page[number]=3");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/api/v1/articles?foo=bar&page[number]=2");
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task When_page_number_is_specified_in_query_string_with_full_page_it_should_render_pagination_links()
        {
            // Arrange
            var articles = _articleFaker.Generate(_defaultPageSize * 3);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Article>(nameof(Article));
                await collection.DeleteManyAsync(Builders<Article>.Filter.Empty);
                await collection.InsertManyAsync(articles);
            });

            var route = "/api/v1/articles?page[number]=3&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?page[number]=3&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/api/v1/articles?page[number]=2&foo=bar");
            responseDocument.Links.Next.Should().Be("http://localhost/api/v1/articles?page[number]=4&foo=bar");
        }
    }
}
