using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Updating.Relationships
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicRemoveFromToManyRelationshipTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicRemoveFromToManyRelationshipTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Cannot_remove_from_HasMany_relationship()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.Performers = _fakers.Performer.Generate(1);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Performer>().InsertOneAsync(existingTrack.Performers[0]);
                await db.GetCollection<MusicTrack>().InsertOneAsync(existingTrack);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                id = existingTrack.Performers[0].StringId
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

        [Fact]
        public async Task Cannot_remove_from_HasManyThrough_relationship()
        {
            // Arrange
            var existingPlaylistMusicTrack = new PlaylistMusicTrack
            {
                Playlist = _fakers.Playlist.Generate(),
                MusicTrack = _fakers.MusicTrack.Generate()
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Playlist>().InsertOneAsync(existingPlaylistMusicTrack.Playlist);
                await db.GetCollection<MusicTrack>().InsertOneAsync(existingPlaylistMusicTrack.MusicTrack);
                await db.GetCollection<PlaylistMusicTrack>().InsertOneAsync(existingPlaylistMusicTrack);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "playlists",
                            id = existingPlaylistMusicTrack.Playlist.StringId,
                            relationship = "tracks"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "musicTracks",
                                id = existingPlaylistMusicTrack.MusicTrack.StringId
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
