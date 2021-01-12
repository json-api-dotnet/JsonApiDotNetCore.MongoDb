using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Deleting
{
    public sealed class DeleteResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
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
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            var route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemsInDatabase = await db.GetCollection<WorkItem>().AsQueryable()
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstOrDefaultAsync();

                workItemsInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Cannot_delete_missing_resource()
        {
            // Arrange
            var route = "/workItems/ffffffffffffffffffffffff";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID 'ffffffffffffffffffffffff' does not exist.");
        }
    }
}
