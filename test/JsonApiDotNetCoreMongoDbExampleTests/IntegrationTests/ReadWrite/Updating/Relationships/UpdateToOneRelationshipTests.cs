using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Updating.Relationships
{
    public sealed class UpdateToOneRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public UpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }
        
        [Fact]
        public async Task Cannot_replace_relationship()
        {
            // Arrange
            var existingGroups = _fakers.WorkItemGroup.Generate(2);
            existingGroups[0].Color = _fakers.RgbColor.Generate();
            existingGroups[1].Color = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<RgbColor>()
                    .InsertManyAsync(existingGroups.Select(workItemGroup => workItemGroup.Color));
                await db.GetCollection<WorkItemGroup>().InsertManyAsync(existingGroups);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = existingGroups[0].Color.StringId
                }
            };

            var route = $"/workItemGroups/{existingGroups[1].StringId}/relationships/color";

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