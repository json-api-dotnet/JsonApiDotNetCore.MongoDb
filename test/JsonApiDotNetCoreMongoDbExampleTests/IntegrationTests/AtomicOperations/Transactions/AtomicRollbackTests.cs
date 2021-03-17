using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    [Collection("AtomicOperationsFixture")]
    public sealed class AtomicRollbackTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicRollbackTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_rollback_created_resource_on_error()
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
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        id = "507f191e810c19729de860ea",
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
            error.Source.Pointer.Should().Be("/atomic:operations[1]");

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                List<Performer> performersInDatabase = await db.GetCollection<Performer>().AsQueryable().ToListAsync();

                performersInDatabase.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Can_rollback_updated_resource_on_error()
        {
            // Arrange
            Performer existingPerformer = _fakers.Performer.Generate();

            string newArtistName = _fakers.Performer.Generate().ArtistName;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Performer>().InsertOneAsync(existingPerformer);
            });

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
            error.Source.Pointer.Should().Be("/atomic:operations[1]");

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                Performer performerInDatabase = await db.GetCollection<Performer>().AsQueryable().FirstWithIdAsync(existingPerformer.Id);

                performerInDatabase.ArtistName.Should().Be(existingPerformer.ArtistName);
            });
        }

        [Fact]
        public async Task Can_rollback_deleted_resource_on_error()
        {
            // Arrange
            Performer existingPerformer = _fakers.Performer.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Performer>();
                await db.GetCollection<Performer>().InsertOneAsync(existingPerformer);
            });

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
            error.Source.Pointer.Should().Be("/atomic:operations[1]");

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                List<Performer> performersInDatabase = await db.GetCollection<Performer>().AsQueryable().ToListAsync();

                performersInDatabase.Should().HaveCount(1);
            });
        }
    }
}
