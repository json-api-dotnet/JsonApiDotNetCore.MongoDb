using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Models;
using MongoDB.Bson;
using Xunit;
using Person = JsonApiDotNetCoreMongoDbExample.Models.Person;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Sorting
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
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertManyAsync(articles);
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
        public async Task Can_sort_descending_by_ID()
        {
            // Arrange
            var ids = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                ids.Add(ObjectId.GenerateNewId().ToString());
            }
            
            var persons = new List<Person>
            {
                new Person
                {
                    Id = ids[2],
                    LastName = "B"
                },
                new Person
                {
                    Id = ids[1],
                    LastName = "A"
                },
                new Person
                {
                    Id = ids[0],
                    LastName = "A"
                },
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Person>();
                await db.GetCollection<Person>().InsertManyAsync(persons);
            });

            var route = "/api/v1/people?sort=lastName,-id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(persons[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(persons[2].StringId);
            responseDocument.ManyData[2].Id.Should().Be(persons[0].StringId);
        }

        [Fact]
        public async Task Sorts_by_ID_if_none_specified()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = ObjectId.GenerateNewId().ToString() },
                new Person { Id = ObjectId.GenerateNewId().ToString() },
                new Person { Id = ObjectId.GenerateNewId().ToString() },
                new Person { Id = ObjectId.GenerateNewId().ToString() }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Person>();
                await db.GetCollection<Person>().InsertManyAsync(persons);
            });

            var route = "/api/v1/people";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            responseDocument.ManyData.Should().HaveCount(4);
            responseDocument.ManyData[0].Id.Should().Be(persons[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(persons[1].StringId);
            responseDocument.ManyData[2].Id.Should().Be(persons[2].StringId);
            responseDocument.ManyData[3].Id.Should().Be(persons[3].StringId);
        }
        
        [Fact]
        public async Task Cannot_sort_on_HasMany_relationship()
        {
            // Arrange
            var blogs = new List<Blog>
            {
                new Blog
                {
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "A"
                        },
                        new Article
                        {
                            Caption = "B"
                        }
                    }
                },
                new Blog
                {
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "C"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Blog>();
                await db.GetCollection<Blog>().InsertManyAsync(blogs);
            });
            
            var route = "/api/v1/blogs?sort=count(articles)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
        
        [Fact]
        public async Task Cannot_sort_on_HasManyThrough_relationship()
        {
            // Arrange
            var route = "/api/v1/articles?sort=-count(tags)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
        
        [Fact]
        public async Task Cannot_sort_on_HasOne_relationship()
        {
            // Arrange
            var route = "/api/v1/articles?sort=-author.lastName";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
    }
}