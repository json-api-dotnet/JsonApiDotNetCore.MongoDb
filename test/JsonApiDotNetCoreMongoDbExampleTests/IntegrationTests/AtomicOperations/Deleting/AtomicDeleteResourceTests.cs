using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Deleting
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicDeleteResourceTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicDeleteResourceTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_delete_existing_resource()
        {
            // Arrange
            Performer existingPerformer = _fakers.Performer.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Performer>().InsertOneAsync(existingPerformer);
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                Performer performerInDatabase = await db.GetCollection<Performer>().AsQueryable().FirstWithIdOrDefaultAsync(existingPerformer.Id);

                performerInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_delete_existing_resources()
        {
            // Arrange
            const int elementCount = 5;

            List<MusicTrack> existingTracks = _fakers.MusicTrack.Generate(elementCount);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<MusicTrack>();
                await db.GetCollection<MusicTrack>().InsertManyAsync(existingTracks);
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                List<MusicTrack> tracksInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().ToListAsync();

                tracksInDatabase.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Cannot_delete_resource_for_unknown_ID()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.EnsureEmptyCollectionAsync<Performer>();
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
                            id = "ffffffffffffffffffffffff"
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be("Resource of type 'performers' with ID 'ffffffffffffffffffffffff' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }
    }
}
