using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings.Sorting
{
    public sealed class SortTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly QueryStringFakers _fakers = new QueryStringFakers();

        public SortTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_sort_in_primary_resources()
        {
            // Arrange
            List<BlogPost> posts = _fakers.BlogPost.Generate(3);
            posts[0].Caption = "B";
            posts[1].Caption = "A";
            posts[2].Caption = "C";

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertManyAsync(posts);
            });

            const string route = "/blogPosts?sort=caption";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(posts[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(posts[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(posts[2].StringId);
        }

        [Fact]
        public async Task Cannot_sort_on_HasMany_relationship()
        {
            // Arrange
            Blog blog = _fakers.Blog.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Blog>().InsertOneAsync(blog);
            });

            const string route = "/blogs?sort=count(posts)";

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
            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            const string route = "/blogPosts?sort=-count(labels)";

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
            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            const string route = "/blogPosts?sort=-author.displayName";

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
            List<WebAccount> accounts = _fakers.WebAccount.Generate(3);
            accounts[0].Id = "5ff752c4f7c9a9a8373991b2";
            accounts[1].Id = "5ff752c3f7c9a9a8373991b1";
            accounts[2].Id = "5ff752c2f7c9a9a8373991b0";

            accounts[0].DisplayName = "B";
            accounts[1].DisplayName = "A";
            accounts[2].DisplayName = "A";

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<WebAccount>();
                await db.GetCollection<WebAccount>().InsertManyAsync(accounts);
            });

            const string route = "/webAccounts?sort=displayName,-id";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(accounts[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(accounts[2].StringId);
            responseDocument.ManyData[2].Id.Should().Be(accounts[0].StringId);
        }

        [Fact]
        public async Task Sorts_by_ID_if_none_specified()
        {
            // Arrange
            List<WebAccount> accounts = _fakers.WebAccount.Generate(4);
            accounts[0].Id = "5ff8a7bcb2a9b83724282718";
            accounts[1].Id = "5ff8a7bcb2a9b83724282717";
            accounts[2].Id = "5ff8a7bbb2a9b83724282716";
            accounts[3].Id = "5ff8a7bdb2a9b83724282719";

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<WebAccount>();
                await db.GetCollection<WebAccount>().InsertManyAsync(accounts);
            });

            const string route = "/webAccounts";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(4);
            responseDocument.ManyData[0].Id.Should().Be(accounts[2].StringId);
            responseDocument.ManyData[1].Id.Should().Be(accounts[1].StringId);
            responseDocument.ManyData[2].Id.Should().Be(accounts[0].StringId);
            responseDocument.ManyData[3].Id.Should().Be(accounts[3].StringId);
        }
    }
}
