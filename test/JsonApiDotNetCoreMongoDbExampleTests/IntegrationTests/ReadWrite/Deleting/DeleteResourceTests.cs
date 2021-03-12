using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Deleting
{
    public sealed class DeleteResourceTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public DeleteResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_delete_existing_resource()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            string route = "/workItems/" + existingWorkItem.StringId;

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                WorkItem workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable().FirstWithIdOrDefaultAsync(existingWorkItem.Id);

                workItemInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Cannot_delete_missing_resource()
        {
            // Arrange
            const string route = "/workItems/ffffffffffffffffffffffff";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be("Resource of type 'workItems' with ID 'ffffffffffffffffffffffff' does not exist.");
        }
    }
}
