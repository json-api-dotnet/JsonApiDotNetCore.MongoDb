using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Creating
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicCreateResourceTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicCreateResourceTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_create_resource()
        {
            // Arrange
            string newArtistName = _fakers.Performer.Generate().ArtistName;
            DateTimeOffset newBornAt = _fakers.Performer.Generate().BornAt;

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
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, AtomicOperationsDocument responseDocument) =
                await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("performers");
            responseDocument.Results[0].SingleData.Attributes["artistName"].Should().Be(newArtistName);
            responseDocument.Results[0].SingleData.Attributes["bornAt"].Should().BeCloseTo(newBornAt);
            responseDocument.Results[0].SingleData.Relationships.Should().BeNull();

            string newPerformerId = responseDocument.Results[0].SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                Performer performerInDatabase = await db.GetCollection<Performer>().AsQueryable().FirstWithIdAsync(newPerformerId);

                performerInDatabase.ArtistName.Should().Be(newArtistName);
                performerInDatabase.BornAt.Should().BeCloseTo(newBornAt);
            });
        }

        [Fact]
        public async Task Can_create_resources()
        {
            // Arrange
            const int elementCount = 5;

            List<MusicTrack> newTracks = _fakers.MusicTrack.Generate(elementCount);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.EnsureEmptyCollectionAsync<MusicTrack>();
            });

            var operationElements = new List<object>(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                operationElements.Add(new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTracks[index].Title,
                            lengthInSeconds = newTracks[index].LengthInSeconds,
                            genre = newTracks[index].Genre,
                            releasedAt = newTracks[index].ReleasedAt
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
            (HttpResponseMessage httpResponse, AtomicOperationsDocument responseDocument) =
                await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                ResourceObject singleData = responseDocument.Results[index].SingleData;

                singleData.Should().NotBeNull();
                singleData.Type.Should().Be("musicTracks");
                singleData.Attributes["title"].Should().Be(newTracks[index].Title);
                singleData.Attributes["lengthInSeconds"].As<decimal?>().Should().BeApproximately(newTracks[index].LengthInSeconds);
                singleData.Attributes["genre"].Should().Be(newTracks[index].Genre);
                singleData.Attributes["releasedAt"].Should().BeCloseTo(newTracks[index].ReleasedAt);
                singleData.Relationships.Should().BeNull();
            }

            string[] newTrackIds = responseDocument.Results.Select(result => result.SingleData.Id).ToArray();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                List<MusicTrack> tracksInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().Where(musicTrack => newTrackIds.Contains(musicTrack.Id))
                    .ToListAsync();

                tracksInDatabase.Should().HaveCount(elementCount);

                for (int index = 0; index < elementCount; index++)
                {
                    MusicTrack trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == newTrackIds[index]);

                    trackInDatabase.Title.Should().Be(newTracks[index].Title);
                    trackInDatabase.LengthInSeconds.Should().BeApproximately(newTracks[index].LengthInSeconds);
                    trackInDatabase.Genre.Should().Be(newTracks[index].Genre);
                    trackInDatabase.ReleasedAt.Should().BeCloseTo(newTracks[index].ReleasedAt);
                }
            });
        }

        [Fact]
        public async Task Can_create_resource_without_attributes_or_relationships()
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
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            attributes = new
                            {
                            },
                            relationship = new
                            {
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

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("performers");
            responseDocument.Results[0].SingleData.Attributes["artistName"].Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["bornAt"].Should().BeCloseTo(default(DateTimeOffset));
            responseDocument.Results[0].SingleData.Relationships.Should().BeNull();

            string newPerformerId = responseDocument.Results[0].SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                Performer performerInDatabase = await db.GetCollection<Performer>().AsQueryable().FirstWithIdAsync(newPerformerId);

                performerInDatabase.ArtistName.Should().BeNull();
                performerInDatabase.BornAt.Should().Be(default);
            });
        }
    }
}
