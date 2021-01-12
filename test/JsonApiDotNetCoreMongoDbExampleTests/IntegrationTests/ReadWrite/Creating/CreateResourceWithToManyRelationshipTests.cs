using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceWithToManyRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public CreateResourceWithToManyRelationshipTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }
        
        [Fact]
        public async Task Cannot_create_HasMany_relationship()
        {
            // Arrange
            var existingUserAccounts = _fakers.UserAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertManyAsync(existingUserAccounts);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[0].StringId
                                },
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[1].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

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