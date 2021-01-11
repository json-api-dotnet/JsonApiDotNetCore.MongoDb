using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Fetching
{
    public sealed class FetchResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
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
            var workItems = _fakers.WorkItem.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<WorkItem>();
                await db.GetCollection<WorkItem>().InsertManyAsync(workItems);
            });

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            var item1 = responseDocument.ManyData.Single(resource => resource.Id == workItems[0].StringId);
            item1.Type.Should().Be("workItems");
            item1.Attributes["description"].Should().Be(workItems[0].Description);
            item1.Attributes["dueAt"].Should().BeCloseTo(workItems[0].DueAt);
            item1.Attributes["priority"].Should().Be(workItems[0].Priority.ToString("G"));
            item1.Relationships.Should().BeNull();

            var item2 = responseDocument.ManyData.Single(resource => resource.Id == workItems[1].StringId);
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
            var workItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            var route = "/workItems/" + workItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

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
            var route = "/workItems/ffffffffffffffffffffffff";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID 'ffffffffffffffffffffffff' does not exist.");
        }
        
        [Fact]
        public async Task Cannot_get_secondary_HasOne_resource()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            workItem.Assignee = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(workItem.Assignee);
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            var route = $"/workItems/{workItem.StringId}/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
        
        [Fact]
        public async Task Cannot_get_secondary_HasMany_resources()
        {
            // Arrange
            var userAccount = _fakers.UserAccount.Generate();
            userAccount.AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertManyAsync(userAccount.AssignedItems);
                await db.GetCollection<UserAccount>().InsertOneAsync(userAccount);
            });

            var route = $"/userAccounts/{userAccount.StringId}/assignedItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_get_secondary_HasManyThrough_resources()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            workItem.WorkItemTags = new[]
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
                await db.GetCollection<WorkTag>()
                    .InsertManyAsync(workItem.WorkItemTags.Select(workItemTag => workItemTag.Tag));
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            var route = $"/workItems/{workItem.StringId}/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
    }
}
