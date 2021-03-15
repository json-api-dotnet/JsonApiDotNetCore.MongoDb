using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings.Pagination
{
    public sealed class PaginationWithTotalCountTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private const string HostPrefix = "http://localhost";
        private const int DefaultPageSize = 5;

        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly QueryStringFakers _fakers = new QueryStringFakers();

        public PaginationWithTotalCountTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
            options.DefaultPageSize = new PageSize(DefaultPageSize);
            options.MaximumPageSize = null;
            options.MaximumPageNumber = null;
        }

        [Fact]
        public async Task Can_paginate_in_primary_resources()
        {
            // Arrange
            List<BlogPost> posts = _fakers.BlogPost.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertManyAsync(posts);
            });

            const string route = "/blogPosts?page[number]=2&page[size]=1";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be(HostPrefix + route);
            responseDocument.Links.First.Should().Be(HostPrefix + "/blogPosts?page[size]=1");
            responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Uses_default_page_number_and_size()
        {
            // Arrange
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(2);

            List<BlogPost> posts = _fakers.BlogPost.Generate(3);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertManyAsync(posts);
            });

            const string route = "/blogPosts";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(posts[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be(HostPrefix + route);
            responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
            responseDocument.Links.Last.Should().Be(HostPrefix + "/blogPosts?page[number]=2");
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
        }

        [Fact]
        public async Task Returns_all_resources_when_paging_is_disabled()
        {
            // Arrange
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = null;

            List<BlogPost> posts = _fakers.BlogPost.Generate(25);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertManyAsync(posts);
            });

            const string route = "/blogPosts";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(25);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be(HostPrefix + route);
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();
        }
    }
}
