using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Creating;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicCreateResourceWithToManyRelationshipTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicCreateResourceWithToManyRelationshipTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;
    }

    [Fact]
    public async Task Cannot_create_ToMany_relationship()
    {
        // Arrange
        Performer existingPerformer = _fakers.Performer.Generate();

        string newTitle = _fakers.MusicTrack.Generate().Title;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Performers.Add(existingPerformer);
            await dbContext.SaveChangesAsync();
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
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTitle
                        },
                        relationships = new
                        {
                            performers = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "performers",
                                        id = existingPerformer.StringId
                                    }
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
