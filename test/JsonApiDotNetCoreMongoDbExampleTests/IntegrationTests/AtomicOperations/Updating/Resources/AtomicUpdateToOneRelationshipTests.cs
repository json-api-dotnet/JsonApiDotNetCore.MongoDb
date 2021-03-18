using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Updating.Resources
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicUpdateToOneRelationshipTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicUpdateToOneRelationshipTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Cannot_create_relationship()
        {
            // Arrange
            Lyric existingLyric = _fakers.Lyric.Generate();
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Lyric>().InsertOneAsync(existingLyric);
                await db.GetCollection<MusicTrack>().InsertOneAsync(existingTrack);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId,
                            relationships = new
                            {
                                track = new
                                {
                                    data = new
                                    {
                                        type = "musicTracks",
                                        id = existingTrack.StringId
                                    }
                                }
                            }
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
