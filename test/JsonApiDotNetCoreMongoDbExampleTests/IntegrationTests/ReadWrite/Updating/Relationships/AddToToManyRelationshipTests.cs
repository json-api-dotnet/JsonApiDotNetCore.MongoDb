using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Updating.Relationships
{
    public sealed class AddToToManyRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public AddToToManyRelationshipTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Cannot_add_to_HasMany_relationship()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            existingWorkItem.Subscribers = _fakers.UserAccount.Generate(2).ToHashSet();

            UserAccount existingSubscriber = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(existingSubscriber);
                await db.GetCollection<UserAccount>().InsertManyAsync(existingWorkItem.Subscribers);
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingWorkItem.Subscribers.ElementAt(1).StringId
                    },
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_add_to_HasManyThrough_relationship()
        {
            // Arrange
            List<WorkItem> existingWorkItems = _fakers.WorkItem.Generate(2);

            existingWorkItems[0].WorkItemTags = new[]
            {
                new WorkItemTag
                {
                    Tag = _fakers.WorkTag.Generate()
                }
            };

            existingWorkItems[1].WorkItemTags = new[]
            {
                new WorkItemTag
                {
                    Tag = _fakers.WorkTag.Generate()
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                IEnumerable<WorkTag> tags = existingWorkItems.SelectMany(workItem => workItem.WorkItemTags.Select(workItemTag => workItemTag.Tag));

                await db.GetCollection<WorkTag>().InsertManyAsync(tags);
                await db.GetCollection<WorkItem>().InsertManyAsync(existingWorkItems);
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "workTags",
                        id = existingWorkItems[0].WorkItemTags.ElementAt(0).Tag.StringId
                    },
                    new
                    {
                        type = "workTags",
                        id = existingWorkItems[1].WorkItemTags.ElementAt(0).Tag.StringId
                    }
                }
            };

            string route = $"/workItems/{existingWorkItems[0].StringId}/relationships/tags";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

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
