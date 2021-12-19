using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Creating;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicCreateResourceTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicCreateResourceTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;

        var options = (JsonApiOptions)fixture.TestContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowClientGeneratedIds = false;
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        string newArtistName = _fakers.Performer.Generate().ArtistName!;
        DateTimeOffset newBornAt = _fakers.Performer.Generate().BornAt;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "performers",
                        attributes = new
                        {
                            artistName = newArtistName,
                            bornAt = newBornAt
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("performers");
            resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newArtistName));
            resource.Attributes.ShouldContainKey("bornAt").With(value => value.Should().Be(newBornAt));
            resource.Relationships.Should().BeNull();
        });

        string newPerformerId = responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Performer performerInDatabase = await dbContext.Performers.FirstWithIdAsync(newPerformerId);

            performerInDatabase.ArtistName.Should().Be(newArtistName);
            performerInDatabase.BornAt.Should().Be(newBornAt);
        });
    }

    [Fact]
    public async Task Can_create_resources()
    {
        // Arrange
        const int elementCount = 5;

        List<MusicTrack> newTracks = _fakers.MusicTrack.Generate(elementCount);

        var operationElements = new List<object>(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            operationElements.Add(new
            {
                op = "add",
                data = new
                {
                    type = "musicTracks",
                    attributes = new
                    {
                        title = newTracks[index].Title,
                        lengthInSeconds = newTracks[index].LengthInSeconds,
                        genre = newTracks[index].Genre,
                        releasedAt = newTracks[index].ReleasedAt
                    }
                }
            });
        }

        var requestBody = new
        {
            atomic__operations = operationElements
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            responseDocument.Results[index].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.ShouldNotBeNull();
                resource.Type.Should().Be("musicTracks");
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTracks[index].Title));

                resource.Attributes.ShouldContainKey("lengthInSeconds")
                    .With(value => value.As<decimal?>().Should().BeApproximately(newTracks[index].LengthInSeconds));

                resource.Attributes.ShouldContainKey("genre").With(value => value.Should().Be(newTracks[index].Genre));
                resource.Attributes.ShouldContainKey("releasedAt").With(value => value.Should().Be(newTracks[index].ReleasedAt));

                resource.Relationships.Should().BeNull();
            });
        }

        string[] newTrackIds = responseDocument.Results.Select(result => result.Data.SingleValue!.Id.ShouldNotBeNull()).ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListWhereAsync(musicTrack => newTrackIds.Contains(musicTrack.Id));

            tracksInDatabase.ShouldHaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                MusicTrack trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == newTrackIds[index]);

                trackInDatabase.Title.Should().Be(newTracks[index].Title);
                trackInDatabase.LengthInSeconds.Should().BeApproximately(newTracks[index].LengthInSeconds);
                trackInDatabase.Genre.Should().Be(newTracks[index].Genre);
                trackInDatabase.ReleasedAt.Should().Be(newTracks[index].ReleasedAt);
            }
        });
    }

    [Fact]
    public async Task Can_create_resource_without_attributes_or_relationships()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "performers",
                        attributes = new
                        {
                        },
                        relationship = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("performers");
            resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().BeNull());
            resource.Attributes.ShouldContainKey("bornAt").With(value => value.Should().Be(default(DateTimeOffset)));
            resource.Relationships.Should().BeNull();
        });

        string newPerformerId = responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Performer performerInDatabase = await dbContext.Performers.FirstWithIdAsync(newPerformerId);

            performerInDatabase.ArtistName.Should().BeNull();
            performerInDatabase.BornAt.Should().Be(default);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_client_generated_ID()
    {
        // Arrange
        MusicTrack newTrack = _fakers.MusicTrack.Generate();
        newTrack.Id = ObjectId.GenerateNewId().ToString();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        id = newTrack.StringId,
                        attributes = new
                        {
                            title = newTrack.Title
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: The use of client-generated IDs is disabled.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/id");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }
}
