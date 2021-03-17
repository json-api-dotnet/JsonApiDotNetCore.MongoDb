using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Creating
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicCreateResourceWithClientGeneratedIdTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicCreateResourceWithClientGeneratedIdTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());

            var options = (JsonApiOptions)fixture.TestContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_side_effects()
        {
            // Arrange
            TextLanguage newLanguage = _fakers.TextLanguage.Generate();
            newLanguage.Id = "507f191e810c19729de860ea";

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.EnsureEmptyCollectionAsync<TextLanguage>();
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
                            type = "textLanguages",
                            id = newLanguage.StringId,
                            attributes = new
                            {
                                isoCode = newLanguage.IsoCode
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
            responseDocument.Results[0].SingleData.Attributes["isoCode"].Should().Be(newLanguage.IsoCode);
            responseDocument.Results[0].SingleData.Attributes.Should().NotContainKey("concurrencyToken");
            responseDocument.Results[0].SingleData.Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                TextLanguage languageInDatabase = await db.GetCollection<TextLanguage>().AsQueryable().FirstWithIdAsync(newLanguage.Id);

                languageInDatabase.IsoCode.Should().Be(newLanguage.IsoCode);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
        {
            // Arrange
            MusicTrack newTrack = _fakers.MusicTrack.Generate();
            newTrack.Id = "5ffcc0d1d69a27c92b8c62dd";

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.EnsureEmptyCollectionAsync<MusicTrack>();
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
                            id = newTrack.StringId,
                            attributes = new
                            {
                                title = newTrack.Title,
                                lengthInSeconds = newTrack.LengthInSeconds
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
                MusicTrack trackInDatabase = await db.GetCollection<MusicTrack>().AsQueryable().FirstWithIdAsync(newTrack.Id);

                trackInDatabase.Title.Should().Be(newTrack.Title);
                trackInDatabase.LengthInSeconds.Should().BeApproximately(newTrack.LengthInSeconds);
            });
        }

        [Fact]
        public async Task Cannot_create_resource_for_existing_client_generated_ID()
        {
            // Arrange
            TextLanguage existingLanguage = _fakers.TextLanguage.Generate();

            TextLanguage languageToCreate = _fakers.TextLanguage.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<TextLanguage>().InsertOneAsync(existingLanguage);
                languageToCreate.Id = existingLanguage.Id;
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
                            type = "textLanguages",
                            id = languageToCreate.StringId,
                            attributes = new
                            {
                                isoCode = languageToCreate.IsoCode
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.Title.Should().Be("Another resource with the specified ID already exists.");
            error.Detail.Should().Be($"Another resource of type 'textLanguages' with ID '{languageToCreate.StringId}' already exists.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }
    }
}
