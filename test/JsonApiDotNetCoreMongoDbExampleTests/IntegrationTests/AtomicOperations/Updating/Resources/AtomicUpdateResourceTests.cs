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
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Updating.Resources
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicUpdateResourceTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicUpdateResourceTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_update_resources()
        {
            // Arrange
            const int elementCount = 5;

            List<MusicTrack> existingTracks = _fakers.MusicTrack.Generate(elementCount);
            string[] newTrackTitles = _fakers.MusicTrack.Generate(elementCount).Select(musicTrack => musicTrack.Title).ToArray();

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
                    op = "update",
                    data = new
                    {
                        type = "musicTracks",
                        id = existingTracks[index].StringId,
                        attributes = new
                        {
                            title = newTrackTitles[index]
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
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                List<MusicTrack> tracksInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().ToListAsync();

                tracksInDatabase.Should().HaveCount(elementCount);

                for (int index = 0; index < elementCount; index++)
                {
                    MusicTrack trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == existingTracks[index].Id);

                    trackInDatabase.Title.Should().Be(newTrackTitles[index]);
                    trackInDatabase.Genre.Should().Be(existingTracks[index].Genre);
                }
            });
        }

        [Fact]
        public async Task Can_update_resource_without_attributes_or_relationships()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<MusicTrack>().InsertOneAsync(existingTrack);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            attributes = new
                            {
                            },
                            relationships = new
                            {
                            }
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
                MusicTrack trackInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.Title.Should().Be(existingTrack.Title);
                trackInDatabase.Genre.Should().Be(existingTrack.Genre);
            });
        }

        [Fact]
        public async Task Can_partially_update_resource_without_side_effects()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

            string newGenre = _fakers.MusicTrack.Generate().Genre;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<MusicTrack>().InsertOneAsync(existingTrack);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            attributes = new
                            {
                                genre = newGenre
                            }
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
                MusicTrack trackInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.Title.Should().Be(existingTrack.Title);
                trackInDatabase.LengthInSeconds.Should().BeApproximately(existingTrack.LengthInSeconds);
                trackInDatabase.Genre.Should().Be(newGenre);
                trackInDatabase.ReleasedAt.Should().BeCloseTo(existingTrack.ReleasedAt);
            });
        }

        [Fact]
        public async Task Can_completely_update_resource_without_side_effects()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

            string newTitle = _fakers.MusicTrack.Generate().Title;
            decimal? newLengthInSeconds = _fakers.MusicTrack.Generate().LengthInSeconds;
            string newGenre = _fakers.MusicTrack.Generate().Genre;
            DateTimeOffset newReleasedAt = _fakers.MusicTrack.Generate().ReleasedAt;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<MusicTrack>().InsertOneAsync(existingTrack);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            attributes = new
                            {
                                title = newTitle,
                                lengthInSeconds = newLengthInSeconds,
                                genre = newGenre,
                                releasedAt = newReleasedAt
                            }
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
                MusicTrack trackInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.Title.Should().Be(newTitle);
                trackInDatabase.LengthInSeconds.Should().BeApproximately(newLengthInSeconds);
                trackInDatabase.Genre.Should().Be(newGenre);
                trackInDatabase.ReleasedAt.Should().BeCloseTo(newReleasedAt);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_side_effects()
        {
            // Arrange
            TextLanguage existingLanguage = _fakers.TextLanguage.Generate();
            string newIsoCode = _fakers.TextLanguage.Generate().IsoCode;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<TextLanguage>().InsertOneAsync(existingLanguage);
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "textLanguages",
                            id = existingLanguage.StringId,
                            attributes = new
                            {
                                isoCode = newIsoCode
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
            responseDocument.Results[0].SingleData.Type.Should().Be("textLanguages");
            responseDocument.Results[0].SingleData.Attributes["isoCode"].Should().Be(newIsoCode);
            responseDocument.Results[0].SingleData.Attributes.Should().NotContainKey("concurrencyToken");
            responseDocument.Results[0].SingleData.Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                TextLanguage languageInDatabase = await db.GetCollection<TextLanguage>().AsQueryable().FirstWithIdAsync(existingLanguage.Id);

                languageInDatabase.IsoCode.Should().Be(newIsoCode);
            });
        }

        [Fact]
        public async Task Cannot_update_resource_for_unknown_ID()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "performers",
                            id = "ffffffffffffffffffffffff",
                            attributes = new
                            {
                            },
                            relationships = new
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
