using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Updating.Relationships;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicUpdateToOneRelationshipTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicUpdateToOneRelationshipTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;
    }

    [Fact]
    public async Task Cannot_create_ManyToOne_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        RecordCompany existingCompany = _fakers.RecordCompany.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.RecordCompanies.Add(existingCompany);
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
                    @ref = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        relationship = "ownedBy"
                    },
                    data = new
                    {
                        type = "recordCompanies",
                        id = existingCompany.StringId
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }
}
