using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceWithToOneRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public CreateResourceWithToOneRelationshipTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }
        
        [Fact]
        public async Task Cannot_create_relationship()
        {
            // Arrange
            var existingGroup = _fakers.WorkItemGroup.Generate();
            existingGroup.Color = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<RgbColor>().InsertOneAsync(existingGroup.Color);
                await db.GetCollection<WorkItemGroup>().InsertOneAsync(existingGroup);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    relationships = new
                    {
                        color = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = existingGroup.Color.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
    }
}