using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.MongoDb.Example.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.Pagination
{
    public sealed class PaginationWithTotalCountTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private const int _defaultPageSize = 5;
        private readonly IntegrationTestContext<Startup> _testContext;
        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>();

        public PaginationWithTotalCountTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
            options.DefaultPageSize = new PageSize(_defaultPageSize);
            options.MaximumPageSize = null;
            options.MaximumPageNumber = null;
            options.AllowUnknownQueryStringParameters = true;
        }

        [Fact]
        public async Task Can_paginate_in_primary_resources()
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
                var collection = db.GetCollection<Article>(nameof(Article));
                await collection.DeleteManyAsync(Builders<Article>.Filter.Empty);
                await collection.InsertManyAsync(articles);
            });

            var route = "/api/v1/articles?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?page[size]=1");
            responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_paginate_in_single_primary_resource()
        {
            // Arrange
            var article = new Article
            {
                Caption = "X"
            };

            await _testContext.RunOnDatabaseAsync(async db => await db.GetCollection<Article>(nameof(Article)).InsertOneAsync(article));

            var route = $"/api/v1/articles/{article.StringId}?page[number]=2";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[number]");
        }
    }
}
