using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample.Models;
using JsonApiDotNetCoreMongoDbExample.Startups;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Filtering
{
    public sealed class FilterDepthTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;

        public FilterDepthTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;
        }

        [Fact]
        public async Task Can_filter_in_primary_resources()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "One"
                },
                new Article
                {
                    Caption = "Two"
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertManyAsync(articles);
            });

            const string route = "/api/v1/articles?filter=equals(caption,'Two')";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
        }

        [Fact]
        public async Task Cannot_filter_in_single_primary_resource()
        {
            // Arrange
            var article = new Article
            {
                Caption = "X"
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            string route = $"/api/v1/articles/{article.StringId}?filter=equals(caption,'Two')";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified filter is invalid.");
            error.Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            error.Source.Parameter.Should().Be("filter");
        }

        [Fact]
        public async Task Cannot_filter_on_HasOne_relationship()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "X",
                    Author = new Author
                    {
                        LastName = "Conner"
                    }
                },
                new Article
                {
                    Caption = "X",
                    Author = new Author
                    {
                        LastName = "Smith"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertManyAsync(articles);
            });

            const string route = "/api/v1/articles?filter=equals(author.lastName,'Smith')";

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
        public async Task Cannot_filter_on_HasMany_relationship()
        {
            // Arrange
            var blogs = new List<Blog>
            {
                new Blog(),
                new Blog
                {
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "X"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Blog>();
                await db.GetCollection<Blog>().InsertManyAsync(blogs);
            });

            const string route = "/api/v1/blogs?filter=greaterThan(count(articles),'0')";

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
        public async Task Cannot_filter_on_HasManyThrough_relationship()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "X"
                },
                new Article
                {
                    Caption = "X",
                    ArticleTags = new HashSet<ArticleTag>
                    {
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "Hot"
                            }
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertManyAsync(articles);
            });

            const string route = "/api/v1/articles?filter=has(tags)";

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
    }
}
