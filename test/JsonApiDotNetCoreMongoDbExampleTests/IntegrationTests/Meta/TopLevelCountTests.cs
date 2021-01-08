using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Meta
{
    public sealed class TopLevelCountTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;

        public TopLevelCountTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Total_Resource_Count_Included_For_Collection()
        {
            // Arrange
            var todoItem = new TodoItem();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<TodoItem>();
                await db.GetCollection<TodoItem>().InsertOneAsync(todoItem);
            });

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().NotBeNull();
            responseDocument.Meta["totalResources"].Should().Be(1);
        }

        [Fact]
        public async Task Total_Resource_Count_Included_For_Empty_Collection()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<TodoItem>();
            });

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().NotBeNull();
            responseDocument.Meta["totalResources"].Should().Be(0);
        }

        [Fact]
        public async Task Total_Resource_Count_Excluded_From_POST_Response()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    attributes = new
                    {
                        description = "Something"
                    }
                }
            };

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Meta.Should().BeNull();
        }

        [Fact]
        public async Task Total_Resource_Count_Excluded_From_PATCH_Response()
        {
            // Arrange
            var todoItem = new TodoItem();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<TodoItem>().InsertOneAsync(todoItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.StringId,
                    attributes = new
                    {
                        description = "Something else"
                    }
                }
            };

            var route = $"/api/v1/todoItems/{todoItem.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().BeNull();
        }
    }
}
