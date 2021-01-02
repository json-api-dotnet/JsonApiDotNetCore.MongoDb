using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Example.Tests.Helpers.Models;
using JsonApiDotNetCore.Serialization.Objects;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite.Fetching
{
    public sealed class FetchResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public FetchResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<Director, string>();
                builder.Add<Movie, string>();
                builder.Add<WorkItem, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoDbRepository<Movie>>();
                services.AddResourceRepository<MongoDbRepository<WorkItem>>();
            });
        }

        [Fact]
        public async Task Can_get_primary_resources()
        {
            // Arrange
            var workItems = _fakers.WorkItem.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<WorkItem>(nameof(WorkItem));
                await collection.DeleteManyAsync(Builders<WorkItem>.Filter.Empty);
                await collection.InsertManyAsync(workItems);
            });

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            var item1 = responseDocument.ManyData.Single(resource => resource.Id == workItems[0].StringId);
            item1.Type.Should().Be("workItems");
            item1.Attributes["description"].Should().Be(workItems[0].Description);
            item1.Attributes["dueAt"].Should().BeCloseTo(workItems[0].DueAt);
            item1.Attributes["priority"].Should().Be(workItems[0].Priority.ToString("G"));

            var item2 = responseDocument.ManyData.Single(resource => resource.Id == workItems[1].StringId);
            item2.Type.Should().Be("workItems");
            item2.Attributes["description"].Should().Be(workItems[1].Description);
            item2.Attributes["dueAt"].Should().BeCloseTo(workItems[1].DueAt);
            item2.Attributes["priority"].Should().Be(workItems[1].Priority.ToString("G"));
        }

        [Fact]
        public async Task Cannot_get_primary_resources_for_unknown_type()
        {
            // Arrange
            var route = "/doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_get_primary_resource_by_ID()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db => await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(workItem));

            var route = "/workItems/" + workItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Id.Should().Be(workItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(workItem.Description);
            responseDocument.SingleData.Attributes["dueAt"].Should().BeCloseTo(workItem.DueAt);
            responseDocument.SingleData.Attributes["priority"].Should().Be(workItem.Priority.ToString("G"));
        }

        [Fact]
        public async Task Cannot_get_primary_resource_for_unknown_type()
        {
            // Arrange
            var route = "/doesNotExist/5f88857c4aa60defec6a4999";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_get_primary_resource_for_unknown_ID()
        {
            // Arrange
            var route = "/workItems/5f88857c4aa60defec6a4999";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID '5f88857c4aa60defec6a4999' does not exist.");
        }
        
        [Fact]
        public async Task Cannot_get_secondary_HasOne_resource()
        {
            // Arrange
            var director = _fakers.Director.Generate();
            await _testContext.RunOnDatabaseAsync(async db =>
                await db.GetCollection<Director>(nameof(Director)).InsertOneAsync(director));

            var movie = _fakers.Movie.Generate();
            movie.Director = director;
            await _testContext.RunOnDatabaseAsync(async db =>
                await db.GetCollection<Movie>(nameof(Movie)).InsertOneAsync(movie));

            var route = $"/movies/{movie.StringId}/director";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("include");
        }
    }
}
