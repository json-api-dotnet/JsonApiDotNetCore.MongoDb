using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Updating.Resources
{
    public sealed class ReplaceToManyRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public ReplaceToManyRelationshipTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }
        
        [Fact]
        public async Task Cannot_replace_HasMany_relationship()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            existingWorkItem.Subscribers = _fakers.UserAccount.Generate(2).ToHashSet();
            
            var existingSubscriber = _fakers.UserAccount.Generate();
            
            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(existingSubscriber);
                await db.GetCollection<UserAccount>().InsertManyAsync(existingWorkItem.Subscribers);
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = "5ff9ec7672a8b3a6c33af4fa"
                                },
                                new
                                {
                                    type = "userAccounts",
                                    id = "5ff9ec7d72a8b3a6c33af4fb"
                                }
                            }
                        }
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
        
        [Fact]
        public async Task Cannot_replace_HasManyThrough_relationship()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            existingWorkItem.WorkItemTags = new[]
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

            var existingTags = _fakers.WorkTag.Generate(2);
            
            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkTag>().InsertManyAsync(existingTags);
                await db.GetCollection<WorkTag>().InsertManyAsync(existingWorkItem.WorkItemTags.Select(workItemTag => workItemTag.Tag));
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        tags = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "workTags",
                                    id = "5ff9ed0e72a8b3a6c33af4fc"
                                },
                                new
                                {
                                    type = "workTags",
                                    id = "5ff9ed0e72a8b3a6c33af4fd"
                                },
                                new
                                {
                                    type = "workTags",
                                    id = "5ff9ed0f72a8b3a6c33af4fe"
                                }
                            }
                        }
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
    }
}