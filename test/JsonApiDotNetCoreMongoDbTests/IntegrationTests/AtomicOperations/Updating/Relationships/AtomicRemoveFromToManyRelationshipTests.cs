using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Updating.Relationships;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicRemoveFromToManyRelationshipTests(AtomicOperationsFixture fixture)
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext = fixture.TestContext;
    private readonly OperationsFakers _fakers = new();

    [Fact]
    public async Task Cannot_remove_from_OneToMany_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        existingTrack.Performers = _fakers.Performer.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Performers.Add(existingTrack.Performers[0]);
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_remove_from_ManyToMany_relationship()
    {
        // Arrange
        Playlist existingPlaylist = _fakers.Playlist.GenerateOne();
        existingPlaylist.Tracks = _fakers.MusicTrack.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingPlaylist.Tracks[0]);
            dbContext.Playlists.Add(existingPlaylist);
            await dbContext.SaveChangesAsync();
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
                        id = existingPlaylist.StringId,
                        relationship = "tracks"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "musicTracks",
                            id = existingPlaylist.Tracks[0].StringId
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }
}
