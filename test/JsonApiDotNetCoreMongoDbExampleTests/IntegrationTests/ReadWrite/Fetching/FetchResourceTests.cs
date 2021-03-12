using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Fetching
{
    public sealed class FetchResourceTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public FetchResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_get_primary_resources()
        {
            // Arrange
            List<WorkItem> workItems = _fakers.WorkItem.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<WorkItem>();
                await db.GetCollection<WorkItem>().InsertManyAsync(workItems);
            });

            const string route = "/workItems";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            ResourceObject item1 = responseDocument.ManyData.Single(resource => resource.Id == workItems[0].StringId);
            item1.Type.Should().Be("workItems");
            item1.Attributes["description"].Should().Be(workItems[0].Description);
            item1.Attributes["dueAt"].Should().BeCloseTo(workItems[0].DueAt);
            item1.Attributes["priority"].Should().Be(workItems[0].Priority.ToString("G"));
            item1.Relationships.Should().BeNull();

            ResourceObject item2 = responseDocument.ManyData.Single(resource => resource.Id == workItems[1].StringId);
            item2.Type.Should().Be("workItems");
            item2.Attributes["description"].Should().Be(workItems[1].Description);
            item2.Attributes["dueAt"].Should().BeCloseTo(workItems[1].DueAt);
            item2.Attributes["priority"].Should().Be(workItems[1].Priority.ToString("G"));
            item2.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_primary_resource_by_ID()
        {
            // Arrange
            WorkItem workItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            string route = "/workItems/" + workItem.StringId;

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Id.Should().Be(workItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(workItem.Description);
            responseDocument.SingleData.Attributes["dueAt"].Should().BeCloseTo(workItem.DueAt);
            responseDocument.SingleData.Attributes["priority"].Should().Be(workItem.Priority.ToString("G"));
            responseDocument.SingleData.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_get_primary_resource_for_unknown_ID()
        {
            // Arrange
            const string route = "/workItems/ffffffffffffffffffffffff";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be("Resource of type 'workItems' with ID 'ffffffffffffffffffffffff' does not exist.");
        }

        [Fact]
        public async Task Cannot_get_secondary_HasOne_resource()
        {
            // Arrange
            WorkItem workItem = _fakers.WorkItem.Generate();
            workItem.Assignee = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(workItem.Assignee);
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            string route = $"/workItems/{workItem.StringId}/assignee";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_get_secondary_HasMany_resources()
        {
            // Arrange
            UserAccount userAccount = _fakers.UserAccount.Generate();
            userAccount.AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertManyAsync(userAccount.AssignedItems);
                await db.GetCollection<UserAccount>().InsertOneAsync(userAccount);
            });

            string route = $"/userAccounts/{userAccount.StringId}/assignedItems";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_get_secondary_HasManyThrough_resources()
        {
            // Arrange
            WorkItem workItem = _fakers.WorkItem.Generate();

            workItem.WorkItemTags = new List<WorkItemTag>
            {
                new WorkItemTag
                {
                    Tag = _fakers.WorkTag.Generate()
                },
                new WorkItemTag
                {
                    Tag = _fakers.WorkTag.Generate()
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkTag>().InsertManyAsync(workItem.WorkItemTags.Select(workItemTag => workItemTag.Tag));
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            string route = $"/workItems/{workItem.StringId}/tags";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
        }
    }
}
