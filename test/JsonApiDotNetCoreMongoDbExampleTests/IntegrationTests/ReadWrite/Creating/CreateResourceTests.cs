using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public CreateResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_create_resource_with_string_ID()
        {
            // Arrange
            WorkItem newWorkItem = _fakers.WorkItem.Generate();
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

            const string route = "/workItems";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(newWorkItem.Description);
            responseDocument.SingleData.Attributes["dueAt"].Should().Be(newWorkItem.DueAt);
            responseDocument.SingleData.Relationships.Should().BeNull();

            string newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                WorkItem workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable().FirstWithIdAsync(newWorkItemId);

                workItemInDatabase.Description.Should().Be(newWorkItem.Description);
                workItemInDatabase.DueAt.Should().Be(newWorkItem.DueAt);
            });

            PropertyInfo property = typeof(WorkItem).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Cannot_create_resource_with_int_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "modelWithIntIds",
                    attributes = new
                    {
                        description = "Test"
                    }
                }
            };

            const string route = "/modelWithIntIds";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.Title.Should().Be("An unhandled error occurred while processing this request.");
            error.Detail.Should().Be("MongoDB can only be used for resources with an 'Id' property of type 'string'.");
        }

        [Fact]
        public async Task Can_create_resource_without_attributes_or_relationships()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                    },
                    relationship = new
                    {
                    }
                }
            };

            const string route = "/workItems";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().BeNull();
            responseDocument.SingleData.Attributes["dueAt"].Should().BeNull();
            responseDocument.SingleData.Relationships.Should().BeNull();

            string newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                WorkItem workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable().FirstWithIdAsync(newWorkItemId);

                workItemInDatabase.Description.Should().BeNull();
                workItemInDatabase.DueAt.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_create_resource_with_unknown_attribute()
        {
            // Arrange
            WorkItem newWorkItem = _fakers.WorkItem.Generate();

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

            const string route = "/workItems";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(newWorkItem.Description);
            responseDocument.SingleData.Relationships.Should().BeNull();

            string newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                WorkItem workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable().FirstWithIdAsync(newWorkItemId);

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
                                id = "ffffffffffffffffffffffff"
                            }
                        }
                    }
                }
            };

            const string route = "/workItems";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes.Should().NotBeEmpty();
            responseDocument.SingleData.Relationships.Should().BeNull();

            string newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                WorkItem workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable().FirstWithIdOrDefaultAsync(newWorkItemId);

                workItemInDatabase.Should().NotBeNull();
            });
        }
    }
}
