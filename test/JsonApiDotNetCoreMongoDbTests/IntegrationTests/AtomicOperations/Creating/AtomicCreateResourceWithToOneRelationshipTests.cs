using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Creating;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicCreateResourceWithToOneRelationshipTests(AtomicOperationsFixture fixture)
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext = fixture.TestContext;
    private readonly OperationsFakers _fakers = new();

    [Fact]
    public async Task Cannot_create_ToOne_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();

        string newLyricText = _fakers.Lyric.Generate().Text;

        await _testContext.RunOnDatabaseAsync(dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            return dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "lyrics",
                        attributes = new
                        {
                            text = newLyricText
                        },
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }
}
