using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
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
            WorkItem workItem = _fakers.WorkItem.Generate();
            workItem.Assignee = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(workItem.Assignee);
                await db.GetCollection<WorkItem>().InsertOneAsync(workItem);
            });

            const string route = "/workItems?include=assignee";

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
