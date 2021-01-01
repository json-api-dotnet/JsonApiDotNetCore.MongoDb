using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.MongoDb.Example.Models;
using MongoDB.Driver;
using Xunit;
using Person = JsonApiDotNetCore.MongoDb.Example.Models.Person;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.Sorting
{
    public sealed class SortTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;

        public SortTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_sort_in_primary_resources()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article {Caption = "B"},
                new Article {Caption = "A"},
                new Article {Caption = "C"}
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Article>(nameof(Article));
                await collection.DeleteManyAsync(Builders<Article>.Filter.Empty);
                await collection.InsertManyAsync(articles);
            });

            var route = "/api/v1/articles?sort=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(articles[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(articles[2].StringId);
        }

        [Fact]
        public async Task Cannot_sort_in_single_primary_resource()
        {
            // Arrange
            var article = new Article
            {
                Caption = "X"
            };

            await _testContext.RunOnDatabaseAsync(async db => await db.GetCollection<Article>(nameof(Article)).InsertOneAsync(article));

            var route = $"/api/v1/articles/{article.StringId}?sort=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified sort is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Cannot_sort_in_single_secondary_resource()
        {
            // Arrange
            var article = new Article
            {
                Caption = "X"
            };

            await _testContext.RunOnDatabaseAsync(async db => await db.GetCollection<Article>(nameof(Article)).InsertOneAsync(article));

            var route = $"/api/v1/articles/{article.StringId}/author?sort=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified sort is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Cannot_sort_on_attribute_with_blocked_capability()
        {
            // Arrange
            var route = "/api/v1/todoItems?sort=achievedDate";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Sorting on the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Sorting on attribute 'achievedDate' is not allowed.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Can_sort_descending_by_ID()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person {LastName = "B"},
                new Person {LastName = "A"},
                new Person {LastName = "A"}
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Person>(nameof(Person));
                await collection.DeleteManyAsync(Builders<Person>.Filter.Empty);
                await collection.InsertManyAsync(persons);
            });

            var route = "/api/v1/people?sort=lastName,-id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            persons.Sort((a, b) => a.LastName.CompareTo(b.LastName) + b.Id.CompareTo(a.Id));
            
            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(persons[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(persons[1].StringId);
            responseDocument.ManyData[2].Id.Should().Be(persons[2].StringId);
        }

        [Fact]
        public async Task Sorts_by_ID_if_none_specified()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person {},
                new Person {},
                new Person {},
                new Person {}
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Person>(nameof(Person));
                await collection.DeleteManyAsync(Builders<Person>.Filter.Empty);
                await collection.InsertManyAsync(persons);
            });

            var route = "/api/v1/people";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            persons.Sort((a, b) => a.Id.CompareTo(b.Id));

            responseDocument.ManyData.Should().HaveCount(4);
            responseDocument.ManyData[0].Id.Should().Be(persons[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(persons[1].StringId);
            responseDocument.ManyData[2].Id.Should().Be(persons[2].StringId);
            responseDocument.ManyData[3].Id.Should().Be(persons[3].StringId);
        }
    }
}