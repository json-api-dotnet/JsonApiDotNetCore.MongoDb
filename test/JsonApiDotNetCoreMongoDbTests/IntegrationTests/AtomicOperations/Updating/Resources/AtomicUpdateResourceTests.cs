using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Updating.Resources;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicUpdateResourceTests(AtomicOperationsFixture fixture)
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext = fixture.TestContext;
    private readonly OperationsFakers _fakers = new();

    [Fact]
    public async Task Can_update_resources()
    {
        // Arrange
        const int elementCount = 5;

        List<MusicTrack> existingTracks = _fakers.MusicTrack.GenerateList(elementCount);
        string[] newTrackTitles = _fakers.MusicTrack.GenerateList(elementCount).Select(musicTrack => musicTrack.Title).ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<MusicTrack>();
            dbContext.MusicTracks.AddRange(existingTracks);
            await dbContext.SaveChangesAsync();
        });

        var operationElements = new List<object>(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            operationElements.Add(new
            {
                op = "update",
                data = new
                {
                    type = "musicTracks",
                    id = existingTracks[index].StringId,
                    attributes = new
                    {
                        title = newTrackTitles[index]
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();

            tracksInDatabase.Should().HaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                MusicTrack trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == existingTracks[index].Id);

                trackInDatabase.Title.Should().Be(newTrackTitles[index]);
                trackInDatabase.Genre.Should().Be(existingTracks[index].Genre);
            }
        });
    }

    [Fact]
    public async Task Can_update_resource_without_attributes_or_relationships()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
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
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        attributes = new
                        {
                        },
                        relationships = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.Title.Should().Be(existingTrack.Title);
            trackInDatabase.Genre.Should().Be(existingTrack.Genre);
        });
    }

    [Fact]
    public async Task Can_partially_update_resource_without_side_effects()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

        string newGenre = _fakers.MusicTrack.GenerateOne().Genre!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
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
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        attributes = new
                        {
                            genre = newGenre
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.Title.Should().Be(existingTrack.Title);
            trackInDatabase.LengthInSeconds.Should().BeApproximately(existingTrack.LengthInSeconds);
            trackInDatabase.Genre.Should().Be(newGenre);
            trackInDatabase.ReleasedAt.Should().Be(existingTrack.ReleasedAt);
        });
    }

    [Fact]
    public async Task Can_completely_update_resource_without_side_effects()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

        string newTitle = _fakers.MusicTrack.GenerateOne().Title;
        decimal? newLengthInSeconds = _fakers.MusicTrack.GenerateOne().LengthInSeconds;
        string newGenre = _fakers.MusicTrack.GenerateOne().Genre!;
        DateTimeOffset newReleasedAt = _fakers.MusicTrack.GenerateOne().ReleasedAt;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
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
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        attributes = new
                        {
                            title = newTitle,
                            lengthInSeconds = newLengthInSeconds,
                            genre = newGenre,
                            releasedAt = newReleasedAt
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.Title.Should().Be(newTitle);
            trackInDatabase.LengthInSeconds.Should().BeApproximately(newLengthInSeconds);
            trackInDatabase.Genre.Should().Be(newGenre);
            trackInDatabase.ReleasedAt.Should().Be(newReleasedAt);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_side_effects()
    {
        // Arrange
        TextLanguage existingLanguage = _fakers.TextLanguage.GenerateOne();
        string newIsoCode = _fakers.TextLanguage.GenerateOne().IsoCode!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextLanguages.Add(existingLanguage);
            await dbContext.SaveChangesAsync();
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
                        type = "textLanguages",
                        id = existingLanguage.StringId,
                        attributes = new
                        {
                            isoCode = newIsoCode
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

        responseDocument.Results.Should().HaveCount(1);

        string isoCode = $"{newIsoCode}{ImplicitlyChangingTextLanguageDefinition.Suffix}";

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("textLanguages");
            resource.Attributes.Should().ContainKey("isoCode").WhoseValue.Should().Be(isoCode);
            resource.Attributes.Should().NotContainKey("isRightToLeft");
            resource.Relationships.Should().BeNull();
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextLanguage languageInDatabase = await dbContext.TextLanguages.FirstWithIdAsync(existingLanguage.Id);
            languageInDatabase.IsoCode.Should().Be(isoCode);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_for_unknown_ID()
    {
        // Arrange
        string performerId = Unknown.StringId.For<Performer, string?>();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "performers",
                        id = performerId,
                        attributes = new
                        {
                        },
                        relationships = new
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'performers' with ID '{performerId}' does not exist.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }
}
