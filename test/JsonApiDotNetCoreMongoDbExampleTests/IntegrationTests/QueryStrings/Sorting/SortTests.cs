using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample.Models;
using JsonApiDotNetCoreMongoDbExample.Startups;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings.Sorting
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
                new Article
                {
                    Caption = "B"
                },
                new Article
                {
                    Caption = "A"
                },
                new Article
                {
                    Caption = "C"
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertManyAsync(articles);
            });

            const string route = "/api/v1/articles?sort=caption";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(articles[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(articles[2].StringId);
        }

        [Fact]
        public async Task Cannot_sort_on_HasMany_relationship()
        {
            // Arrange
            var blog = new Blog();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Blog>().InsertOneAsync(blog);
            });

            const string route = "/api/v1/blogs?sort=count(articles)";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_sort_on_HasManyThrough_relationship()
        {
            // Arrange
            var article = new Article();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            const string route = "/api/v1/articles?sort=-count(tags)";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_sort_on_HasOne_relationship()
        {
            // Arrange
            var article = new Article();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            const string route = "/api/v1/articles?sort=-author.lastName";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Relationships are not supported when using MongoDB.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_sort_descending_by_ID()
        {
            // Arrange
            var people = new List<Person>
            {
                new Person
                {
                    Id = "5ff752c4f7c9a9a8373991b2",
                    LastName = "B"
                },
                new Person
                {
                    Id = "5ff752c3f7c9a9a8373991b1",
                    LastName = "A"
                },
                new Person
                {
                    Id = "5ff752c2f7c9a9a8373991b0",
                    LastName = "A"
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Person>();
                await db.GetCollection<Person>().InsertManyAsync(people);
            });

            const string route = "/api/v1/people?sort=lastName,-id";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(people[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(people[2].StringId);
            responseDocument.ManyData[2].Id.Should().Be(people[0].StringId);
        }

        [Fact]
        public async Task Sorts_by_ID_if_none_specified()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person
                {
                    Id = "5ff8a7bcb2a9b83724282718"
                },
                new Person
                {
                    Id = "5ff8a7bcb2a9b83724282717"
                },
                new Person
                {
                    Id = "5ff8a7bbb2a9b83724282716"
                },
                new Person
                {
                    Id = "5ff8a7bdb2a9b83724282719"
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Person>();
                await db.GetCollection<Person>().InsertManyAsync(persons);
            });

            const string route = "/api/v1/people";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(4);
            responseDocument.ManyData[0].Id.Should().Be(persons[2].StringId);
            responseDocument.ManyData[1].Id.Should().Be(persons[1].StringId);
            responseDocument.ManyData[2].Id.Should().Be(persons[0].StringId);
            responseDocument.ManyData[3].Id.Should().Be(persons[3].StringId);
        }
    }
}
