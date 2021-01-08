using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Tag = JsonApiDotNetCoreMongoDbExample.Models.Tag;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Filtering
{
    public sealed class FilterDepthTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;

        public FilterDepthTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
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

            var route = "/api/v1/articles?filter=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

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

            var route = $"/api/v1/articles/{article.StringId}?filter=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified filter is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter");
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

            var route = "/api/v1/articles?filter=equals(author.lastName,'Smith')";

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

            var route = "/api/v1/blogs?filter=greaterThan(count(articles),'0')";

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

            var route = "/api/v1/articles?filter=has(tags)";

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
