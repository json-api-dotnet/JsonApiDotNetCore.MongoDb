using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.SparseFieldSets;

public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<TestableStartup, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SparseFieldSetTests(IntegrationTestContext<TestableStartup, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogPostsController>();
        testContext.UseController<WebAccountsController>();
        testContext.UseController<BlogsController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddSingleton<ResourceCaptureStore>();

            services.AddResourceRepository<ResultCapturingRepository<Blog, string?>>();
            services.AddResourceRepository<ResultCapturingRepository<BlogPost, string?>>();
            services.AddResourceRepository<ResultCapturingRepository<WebAccount, string?>>();
        });
    }

    [Fact]
    public async Task Cannot_select_fields_with_relationship_in_primary_resources()
    {
        // Arrange
        const string route = "/blogPosts?fields[blogPosts]=caption,author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_attribute_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_select_relationship_in_primary_resources()
    {
        // Arrange
        const string route = "/blogPosts?fields[blogPosts]=author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_attribute_in_primary_resource_by_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?fields[blogPosts]=url";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("url").With(value => value.Should().Be(post.Url));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Url.Should().Be(post.Url);
        postCaptured.Caption.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_select_fields_of_ManyToOne_relationship()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?fields[webAccounts]=displayName,emailAddress,preferences";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_select_fields_of_OneToMany_relationship()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}?fields[blogPosts]=caption,labels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_select_fields_of_ManyToMany_relationship()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?fields[labels]=color";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=id,caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_empty_fieldset()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Retrieves_all_properties_when_fieldset_contains_readonly_attribute()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.Generate();
        blog.IsPublished = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}?fields[blogs]=showAdvertisements";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("showAdvertisements").With(value => value.Should().Be(blog.ShowAdvertisements));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).And.Subject.Single();
        blogCaptured.ShowAdvertisements.Should().Be(blog.ShowAdvertisements);
        blogCaptured.IsPublished.Should().Be(blog.IsPublished);
        blogCaptured.Title.Should().Be(blog.Title);
    }
}
