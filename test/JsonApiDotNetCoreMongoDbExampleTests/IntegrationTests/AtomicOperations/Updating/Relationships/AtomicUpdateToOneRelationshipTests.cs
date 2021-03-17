using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Updating.Relationships
{
    public sealed class AtomicUpdateToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicUpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            testContext.StartMongoDbInSingleNodeReplicaSetMode = true;
            testContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Cannot_create_relationship()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            RecordCompany existingCompany = _fakers.RecordCompany.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<MusicTrack>().InsertOneAsync(existingTrack);
                await db.GetCollection<RecordCompany>().InsertOneAsync(existingCompany);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "ownedBy"
                        },
                        data = new
                        {
                            type = "recordCompanies",
                            id = existingCompany.StringId
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }
    }
}
