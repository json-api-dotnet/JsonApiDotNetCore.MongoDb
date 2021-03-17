using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    public sealed class AtomicTransactionConsistencyTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;

        public AtomicTransactionConsistencyTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            testContext.StartMongoDbInSingleNodeReplicaSetMode = true;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddControllersFromExampleProject();

                services.AddResourceRepository<ContainerTypeToHidePerformerRepositoryFromAutoDiscovery.PerformerRepository>();
                services.AddResourceRepository<ContainerTypeToHideMusicTrackRepositoryFromAutoDiscovery.MusicTrackRepository>();
                services.AddResourceRepository<ContainerTypeToHideLyricRepositoryFromAutoDiscovery.LyricRepository>();
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
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Unsupported resource type in atomic:operations request.");
            error.Detail.Should().Be("Operations on resources of type 'performers' cannot be used because transaction support is unavailable.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_use_transactional_repository_without_active_transaction()
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
                            type = "musicTracks",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Unsupported combination of resource types in atomic:operations request.");
            error.Detail.Should().Be("All operations need to participate in a single shared transaction, which is not the case for this request.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_use_distributed_transaction()
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
                            type = "lyrics",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Unsupported combination of resource types in atomic:operations request.");
            error.Detail.Should().Be("All operations need to participate in a single shared transaction, which is not the case for this request.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }
    }
}
