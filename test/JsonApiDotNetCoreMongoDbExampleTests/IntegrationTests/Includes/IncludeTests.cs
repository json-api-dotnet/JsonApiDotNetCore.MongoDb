using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Includes
{
    public sealed class IncludeTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public IncludeTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Cannot_include_in_primary_resources()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            workItem.Assignee = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(workItem.Assignee);
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            var route = "/workItems?include=assignee";
            
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