using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings.SparseFieldSets
{
    public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly QueryStringFakers _fakers = new QueryStringFakers();

        public SparseFieldSetTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<ResourceCaptureStore>();

                services.AddResourceRepository<ResultCapturingRepository<Blog>>();
                services.AddResourceRepository<ResultCapturingRepository<BlogPost>>();
                services.AddResourceRepository<ResultCapturingRepository<WebAccount>>();
            });
        }

        [Fact]
        public async Task Cannot_select_fields_with_relationship_in_primary_resources()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            const string route = "/blogPosts?fields[blogPosts]=caption,author";

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
        public async Task Can_select_attribute_in_primary_resources()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            const string route = "/blogPosts?fields[blogPosts]=caption";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(post.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(post.Caption);
            responseDocument.ManyData[0].Relationships.Should().BeNull();

            var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
            postCaptured.Caption.Should().Be(post.Caption);
            postCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_select_relationship_in_primary_resources()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            const string route = "/blogPosts?fields[blogPosts]=author";

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
        public async Task Can_select_attribute_in_primary_resource_by_ID()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            string route = $"/blogPosts/{post.StringId}?fields[blogPosts]=url";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(post.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["url"].Should().Be(post.Url);
            responseDocument.SingleData.Relationships.Should().BeNull();

            var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
            postCaptured.Url.Should().Be(post.Url);
            postCaptured.Caption.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_select_fields_of_HasOne_relationship()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            string route = $"/blogPosts/{post.StringId}?fields[webAccounts]=displayName,emailAddress,preferences";

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
        public async Task Cannot_select_fields_of_HasMany_relationship()
        {
            // Arrange
            WebAccount account = _fakers.WebAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WebAccount>().InsertOneAsync(account);
            });

            string route = $"/webAccounts/{account.StringId}?fields[blogPosts]=caption,labels";

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
        public async Task Cannot_select_fields_of_HasManyThrough_relationship()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            string route = $"/blogPosts/{post.StringId}?fields[labels]=color";

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
        public async Task Can_select_ID()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            BlogPost post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<BlogPost>();
                await db.GetCollection<BlogPost>().InsertOneAsync(post);
            });

            const string route = "/blogPosts?fields[blogPosts]=id,caption";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(post.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(post.Caption);
            responseDocument.ManyData[0].Relationships.Should().BeNull();

            var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
            postCaptured.Id.Should().Be(post.Id);
            postCaptured.Caption.Should().Be(post.Caption);
            postCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Retrieves_all_properties_when_fieldset_contains_readonly_attribute()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            Blog blog = _fakers.Blog.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Blog>().InsertOneAsync(blog);
            });

            string route = $"/blogs/{blog.StringId}?fields[blogs]=showAdvertisements";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(blog.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["showAdvertisements"].Should().Be(blog.ShowAdvertisements);
            responseDocument.SingleData.Relationships.Should().BeNull();

            var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).And.Subject.Single();
            blogCaptured.ShowAdvertisements.Should().Be(blogCaptured.ShowAdvertisements);
            blogCaptured.Title.Should().Be(blog.Title);
        }
    }
}
