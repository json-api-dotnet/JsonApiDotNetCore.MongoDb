using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Transactions;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicRollbackTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicRollbackTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;

        fixture.TestContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Can_rollback_created_resource_on_error()
    {
        // Arrange
        string newArtistName = _fakers.Performer.Generate().ArtistName!;
        DateTimeOffset newBornAt = _fakers.Performer.Generate().BornAt;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Performer>();
        });

        string unknownPerformerId = Unknown.StringId.For<Performer, string?>();

        var requestBody = new
        {
            atomic__operations = new object[]
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
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "performers",
                        id = unknownPerformerId
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
        error.Detail.Should().Be($"Resource of type 'performers' with ID '{unknownPerformerId}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[1]");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<Performer> performersInDatabase = await dbContext.Performers.ToListAsync();
            performersInDatabase.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_rollback_updated_resource_on_error()
    {
        // Arrange
        Performer existingPerformer = _fakers.Performer.Generate();

        string newArtistName = _fakers.Performer.Generate().ArtistName!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Performers.Add(existingPerformer);
            await dbContext.SaveChangesAsync();
        });

        string unknownPerformerId = Unknown.StringId.For<Performer, string?>();

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "performers",
                        id = existingPerformer.StringId,
                        attributes = new
                        {
                            artistName = newArtistName
                        }
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "performers",
                        id = unknownPerformerId
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
        error.Detail.Should().Be($"Resource of type 'performers' with ID '{unknownPerformerId}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[1]");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Performer performerInDatabase = await dbContext.Performers.FirstWithIdAsync(existingPerformer.Id);

            performerInDatabase.ArtistName.Should().Be(existingPerformer.ArtistName);
        });
    }

    [Fact]
    public async Task Can_rollback_deleted_resource_on_error()
    {
        // Arrange
        Performer existingPerformer = _fakers.Performer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Performer>();
            dbContext.Performers.Add(existingPerformer);
            await dbContext.SaveChangesAsync();
        });

        string unknownPerformerId = Unknown.StringId.For<Performer, string?>();

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "performers",
                        id = existingPerformer.StringId
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "performers",
                        id = unknownPerformerId
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
        error.Detail.Should().Be($"Resource of type 'performers' with ID '{unknownPerformerId}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[1]");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<Performer> performersInDatabase = await dbContext.Performers.ToListAsync();

            performersInDatabase.Should().HaveCount(1);
        });
    }
}
