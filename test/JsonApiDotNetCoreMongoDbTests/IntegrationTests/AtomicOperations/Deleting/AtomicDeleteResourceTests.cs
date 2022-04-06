using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Deleting;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicDeleteResourceTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicDeleteResourceTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;
    }

    [Fact]
    public async Task Can_delete_existing_resource()
    {
        // Arrange
        Performer existingPerformer = _fakers.Performer.Generate();

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
                    op = "remove",
                    @ref = new
                    {
                        type = "performers",
                        id = existingPerformer.StringId
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
            Performer? performerInDatabase = await dbContext.Performers.FirstWithIdOrDefaultAsync(existingPerformer.Id);

            performerInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_existing_resources()
    {
        // Arrange
        const int elementCount = 5;

        List<MusicTrack> existingTracks = _fakers.MusicTrack.Generate(elementCount);

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
                op = "remove",
                @ref = new
                {
                    type = "musicTracks",
                    id = existingTracks[index].StringId
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

            tracksInDatabase.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Cannot_delete_resource_for_unknown_ID()
    {
        // Arrange
        string performerId = Unknown.StringId.For<Performer, string?>();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "performers",
                        id = performerId
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'performers' with ID '{performerId}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }
}
