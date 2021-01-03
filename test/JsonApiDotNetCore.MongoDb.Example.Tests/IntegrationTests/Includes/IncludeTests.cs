using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Example.Tests.Helpers.Models;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.Includes
{
    public sealed class IncludeTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;

        public IncludeTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<Director, string>();
                builder.Add<Movie, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoDbRepository<Movie>>();
            });
            
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumIncludeDepth = null;
        }

        [Fact]
        public async Task Cannot_include_in_primary_resources()
        {
            // Arrange
            var director = new Director
            {
                Name = "John Smith"
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Director>(nameof(Director));
                await collection.DeleteManyAsync(Builders<Director>.Filter.Empty);
                await collection.InsertOneAsync(director);
            });

            var movie = new Movie
            {
                Name = "Movie 1",
                Director = director
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Movie>(nameof(Movie));
                await collection.DeleteManyAsync(Builders<Movie>.Filter.Empty);
                await collection.InsertOneAsync(movie);
            });

            var route = "/movies?include=director";
            
            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
        }
    }
}