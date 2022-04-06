using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.LocalIds;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicLocalIdTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicLocalIdTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;
    }

    [Fact]
    public async Task Can_update_resource_using_local_ID()
    {
        // Arrange
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;
        string newTrackGenre = _fakers.MusicTrack.Generate().Genre!;

        const string trackLocalId = "track-1";

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        lid = trackLocalId,
                        attributes = new
                        {
                            title = newTrackTitle
                        }
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "musicTracks",
                        lid = trackLocalId,
                        attributes = new
                        {
                            genre = newTrackGenre
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(2);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Lid.Should().BeNull();
            resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            resource.Attributes.ShouldContainKey("genre").With(value => value.Should().BeNull());
        });

        responseDocument.Results[1].Data.Value.Should().BeNull();

        string newTrackId = responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(newTrackId);

            trackInDatabase.Title.Should().Be(newTrackTitle);
            trackInDatabase.Genre.Should().Be(newTrackGenre);
        });
    }

    [Fact]
    public async Task Can_delete_resource_using_local_ID()
    {
        // Arrange
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;

        const string trackLocalId = "track-1";

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        lid = trackLocalId,
                        attributes = new
                        {
                            title = newTrackTitle
                        }
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "musicTracks",
                        lid = trackLocalId
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(2);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Lid.Should().BeNull();
            resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
        });

        responseDocument.Results[1].Data.Value.Should().BeNull();

        string newTrackId = responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack? trackInDatabase = await dbContext.MusicTracks.FirstWithIdOrDefaultAsync(newTrackId);

            trackInDatabase.Should().BeNull();
        });
    }
}
