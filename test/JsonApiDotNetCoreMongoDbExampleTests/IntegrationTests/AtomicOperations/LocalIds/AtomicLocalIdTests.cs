using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.LocalIds
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicLocalIdTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicLocalIdTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_update_resource_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newTrackGenre = _fakers.MusicTrack.Generate().Genre;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.EnsureEmptyCollectionAsync<MusicTrack>();
            });

            const string trackLocalId = "track-1";

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
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                genre = newTrackGenre
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, AtomicOperationsDocument responseDocument) =
                await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);
            responseDocument.Results[0].SingleData.Attributes["genre"].Should().BeNull();

            responseDocument.Results[1].Data.Should().BeNull();

            string newTrackId = responseDocument.Results[0].SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                MusicTrack trackInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);
                trackInDatabase.Genre.Should().Be(newTrackGenre);
            });
        }

        [Fact]
        public async Task Can_delete_resource_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.EnsureEmptyCollectionAsync<MusicTrack>();
            });

            const string trackLocalId = "track-1";

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
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, AtomicOperationsDocument responseDocument) =
                await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].Data.Should().BeNull();

            string newTrackId = responseDocument.Results[0].SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                MusicTrack trackInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().FirstWithIdOrDefaultAsync(newTrackId);

                trackInDatabase.Should().BeNull();
            });
        }
    }
}
