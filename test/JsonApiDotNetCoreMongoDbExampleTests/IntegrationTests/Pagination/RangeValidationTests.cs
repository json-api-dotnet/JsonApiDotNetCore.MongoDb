using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Pagination
{
    public sealed class RangeValidationTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;
        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>();

        private const int _defaultPageSize = 5;

        public RangeValidationTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;
            
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(_defaultPageSize);
            options.MaximumPageSize = null;
            options.MaximumPageNumber = null;
        }
        
        [Fact]
        public async Task When_page_number_is_too_high_it_must_return_empty_set_of_resources()
        {
            // Arrange
            var todoItems = _todoItemFaker.Generate(3);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<TodoItem>();
                await db.GetCollection<TodoItem>().InsertManyAsync(todoItems);
            });

            var route = "/api/v1/todoItems?sort=id&page[size]=3&page[number]=2";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().BeEmpty();
        }

        [Fact]
        public async Task When_page_size_is_zero_it_must_succeed()
        {
            // Arrange
            var route = "/api/v1/todoItems?page[size]=0";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_page_size_is_positive_it_must_succeed()
        {
            // Arrange
            var route = "/api/v1/todoItems?page[size]=50";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }
    }
}
