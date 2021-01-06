using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public CreateResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<RgbColor, string>();
                builder.Add<WorkItem, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoDbRepository<RgbColor>>();
                services.AddResourceRepository<MongoDbRepository<WorkItem>>();
            });

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;
            options.AllowClientGeneratedIds = false;
        }

        [Fact]
        public async Task Can_create_resource_with_ID()
        {
            // Arrange
            var newWorkItem = _fakers.WorkItem.Generate();
            newWorkItem.DueAt = null;

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = newWorkItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(newWorkItem.Description);
            responseDocument.SingleData.Attributes["dueAt"].Should().Be(newWorkItem.DueAt);

            var newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                        .FindAsync(Builders<WorkItem>.Filter.Eq(w => w.Id, newWorkItemId)))
                        .FirstAsync();

                workItemInDatabase.Description.Should().Be(newWorkItem.Description);
                workItemInDatabase.DueAt.Should().Be(newWorkItem.DueAt);
            });

            var property = typeof(WorkItem).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Can_create_resource_with_unknown_attribute()
        {
            // Arrange
            var newWorkItem = _fakers.WorkItem.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        doesNotExist = "ignored",
                        description = newWorkItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(newWorkItem.Description);

            var newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                    .FindAsync(Builders<WorkItem>.Filter.Eq(workItem => workItem.Id, newWorkItemId)))
                    .FirstAsync();

                workItemInDatabase.Description.Should().Be(newWorkItem.Description);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_unknown_relationship()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        doesNotExist = new
                        {
                            data = new
                            {
                                type = "doesNotExist",
                                id = 12345678
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                    .FindAsync(Builders<WorkItem>.Filter.Eq(workItem => workItem.Id, newWorkItemId)))
                    .FirstOrDefaultAsync();

                workItemInDatabase.Should().NotBeNull();
            });
        }

        [Fact]
        public async Task Cannot_create_resource_with_incompatible_attribute_value()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        dueAt = "not-a-valid-time"
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Failed to convert 'not-a-valid-time' of type 'String' to type 'Nullable`1'. - Request body: <<");
        }
    }
}
