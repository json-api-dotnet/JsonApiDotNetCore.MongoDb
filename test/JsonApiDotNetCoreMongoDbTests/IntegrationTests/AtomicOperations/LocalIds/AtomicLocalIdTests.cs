using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.LocalIds;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicLocalIdTests(AtomicOperationsFixture fixture)
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext = fixture.TestContext;
    private readonly OperationsFakers _fakers = new();

    [Fact]
    public async Task Can_update_resource_using_local_ID()
    {
        // Arrange
        string newTrackTitle = _fakers.MusicTrack.GenerateOne().Title;
        string newTrackGenre = _fakers.MusicTrack.GenerateOne().Genre!;

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

        responseDocument.Results.Should().HaveCount(2);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Lid.Should().BeNull();
            resource.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(newTrackTitle);
            resource.Attributes.Should().ContainKey("genre").WhoseValue.Should().BeNull();
        });

        responseDocument.Results[1].Data.Value.Should().BeNull();

        string newTrackId = responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject;

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
        string newTrackTitle = _fakers.MusicTrack.GenerateOne().Title;

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

        responseDocument.Results.Should().HaveCount(2);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Lid.Should().BeNull();
            resource.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(newTrackTitle);
        });

        responseDocument.Results[1].Data.Value.Should().BeNull();

        string newTrackId = responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack? trackInDatabase = await dbContext.MusicTracks.FirstWithIdOrDefaultAsync(newTrackId);

            trackInDatabase.Should().BeNull();
        });
    }
}
