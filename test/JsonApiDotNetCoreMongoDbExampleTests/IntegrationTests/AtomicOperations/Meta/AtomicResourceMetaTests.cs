using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Meta
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicResourceMetaTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicResourceMetaTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Returns_resource_meta_in_create_resource_with_side_effects()
        {
            // Arrange
            string newTitle1 = _fakers.MusicTrack.Generate().Title;
            string newTitle2 = _fakers.MusicTrack.Generate().Title;

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
                            attributes = new
                            {
                                title = newTitle1,
                                releasedAt = 1.January(2018)
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            attributes = new
                            {
                                title = newTitle2,
                                releasedAt = 23.August(1994)
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

            responseDocument.Results[0].SingleData.Meta.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Meta["Copyright"].Should().Be("(C) 2018. All rights reserved.");

            responseDocument.Results[1].SingleData.Meta.Should().HaveCount(1);
            responseDocument.Results[1].SingleData.Meta["Copyright"].Should().Be("(C) 1994. All rights reserved.");
        }

        [Fact]
        public async Task Returns_resource_meta_in_update_resource_with_side_effects()
        {
            // Arrange
            TextLanguage existingLanguage = _fakers.TextLanguage.Generate();

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
            responseDocument.Results[0].SingleData.Meta.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Meta["Notice"].Should().Be(TextLanguageMetaDefinition.NoticeText);
        }
    }
}
