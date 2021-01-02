using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.MongoDb.Example.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.SparseFieldSets
{
    public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;
        private readonly Faker<User> _userFaker;

        public SparseFieldSetTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<ResourceCaptureStore>();

                services.AddResourceRepository<ResultCapturingRepository<Blog>>();
                services.AddResourceRepository<ResultCapturingRepository<Article>>();
                services.AddResourceRepository<ResultCapturingRepository<Author>>();
                services.AddResourceRepository<ResultCapturingRepository<TodoItem>>();

                services.AddScoped<IResourceService<Article, string>, JsonApiResourceService<Article, string>>();
            });
            
            _userFaker = new Faker<User>()
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Password, f => f.Internet.Password());
        }

        [Fact]
        public async Task Can_select_fields_in_primary_resources()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Article>(nameof(Article));
                await collection.DeleteManyAsync(Builders<Article>.Filter.Empty);
                await collection.InsertOneAsync(article);
            });

            var route = "/api/v1/articles?fields[articles]=caption"; // TODO: once relationships are implemented select author field too

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(article.Caption);

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Caption.Should().Be(article.Caption);
            articleCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_attribute_in_primary_resources()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Article>(nameof(Article));
                await collection.DeleteManyAsync(Builders<Article>.Filter.Empty);
                await collection.InsertOneAsync(article);
            });

            var route = "/api/v1/articles?fields[articles]=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(article.Caption);
            responseDocument.ManyData[0].Relationships.Should().BeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Caption.Should().Be(article.Caption);
            articleCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_fields_in_primary_resource_by_ID()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async db => await db.GetCollection<Article>(nameof(Article)).InsertOneAsync(article));

            var route = $"/api/v1/articles/{article.StringId}?fields[articles]=url"; // TODO: once relationships are implemented select author field too

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["url"].Should().Be(article.Url);

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Url.Should().Be(article.Url);
            articleCaptured.Caption.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_ID()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<Article>(nameof(Article));
                await collection.DeleteManyAsync(Builders<Article>.Filter.Empty);
                await collection.InsertOneAsync(article);
            });

            var route = "/api/v1/articles?fields[articles]=id,caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(article.Caption);
            responseDocument.ManyData[0].Relationships.Should().BeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Id.Should().Be(article.Id);
            articleCaptured.Caption.Should().Be(article.Caption);
            articleCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_select_on_unknown_resource_type()
        {
            // Arrange
            var route = "/api/v1/people?fields[doesNotExist]=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified fieldset is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Resource type 'doesNotExist' does not exist.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("fields[doesNotExist]");
        }

        [Fact]
        public async Task Cannot_select_attribute_with_blocked_capability()
        {
            // Arrange
            var user = _userFaker.Generate();

            var route = $"/api/v1/users/{user.Id}?fields[users]=password";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Retrieving the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Retrieving the attribute 'password' is not allowed.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("fields[users]");
        }
        
        [Fact]
        public async Task Cannot_retrieve_all_properties_when_fieldset_contains_readonly_attribute()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var todoItem = new TodoItem
            {
                Description = "Pending work..."
            };

            await _testContext.RunOnDatabaseAsync(async db =>
                await db.GetCollection<TodoItem>(nameof(TodoItem)).InsertOneAsync(todoItem));

            var route = $"/api/v1/todoItems/{todoItem.StringId}?fields[todoItems]=calculatedValue";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);
        }

    }
}
