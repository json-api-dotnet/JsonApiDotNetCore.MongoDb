using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample.Models;
using JsonApiDotNetCoreMongoDbExample.Startups;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Pagination
{
    public sealed class RangeValidationTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private const int DefaultPageSize = 5;

        private readonly IntegrationTestContext<Startup> _testContext;
        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>();

        public RangeValidationTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(DefaultPageSize);
            options.MaximumPageSize = null;
            options.MaximumPageNumber = null;
        }

        [Fact]
        public async Task When_page_number_is_too_high_it_must_return_empty_set_of_resources()
        {
            // Arrange
            List<TodoItem> todoItems = _todoItemFaker.Generate(3);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<TodoItem>();
                await db.GetCollection<TodoItem>().InsertManyAsync(todoItems);
            });

            const string route = "/api/v1/todoItems?sort=id&page[size]=3&page[number]=2";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().BeEmpty();
        }

        [Fact]
        public async Task When_page_size_is_zero_it_must_succeed()
        {
            // Arrange
            const string route = "/api/v1/todoItems?page[size]=0";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_page_size_is_positive_it_must_succeed()
        {
            // Arrange
            const string route = "/api/v1/todoItems?page[size]=50";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }
    }
}
