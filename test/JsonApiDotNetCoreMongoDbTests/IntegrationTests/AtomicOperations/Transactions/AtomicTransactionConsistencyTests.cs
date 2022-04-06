using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Transactions;

public sealed class AtomicTransactionConsistencyTests : IClassFixture<IntegrationTestContext<TestableStartup, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicTransactionConsistencyTests(IntegrationTestContext<TestableStartup, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        testContext.StartMongoDbInSingleNodeReplicaSetMode = true;

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddSingleton<ResourceDefinitionHitCounter>();

            services.AddResourceRepository<ContainerTypeToHideFromAutoDiscovery.PerformerRepository>();
            services.AddResourceRepository<ContainerTypeToHideFromAutoDiscovery.MusicTrackRepository>();
            services.AddResourceRepository<ContainerTypeToHideFromAutoDiscovery.LyricRepository>();
        });
    }

    [Fact]
    public async Task Cannot_use_non_transactional_repository()
    {
        // Arrange
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
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported resource type in atomic:operations request.");
        error.Detail.Should().Be("Operations on resources of type 'performers' cannot be used because transaction support is unavailable.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_use_transactional_repository_without_active_transaction()
    {
        // Arrange
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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
                        attributes = new
                        {
                            title = newTrackTitle
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported combination of resource types in atomic:operations request.");
        error.Detail.Should().Be("All operations need to participate in a single shared transaction, which is not the case for this request.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_use_distributed_transaction()
    {
        // Arrange
        string newLyricText = _fakers.Lyric.Generate().Text;

        var requestBody = new
        {
            atomic__operations = new object[]
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
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported combination of resource types in atomic:operations request.");
        error.Detail.Should().Be("All operations need to participate in a single shared transaction, which is not the case for this request.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }
}
