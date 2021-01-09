using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.SparseFieldSets
{
    public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;

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
            });
        }

        [Fact]
        public async Task Cannot_select_fields_with_relationship_in_primary_resources()
        {
            // Arrange
            var article = new Article();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            var route = "/api/v1/articles?fields[articles]=caption,author";

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
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertOneAsync(article);
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
        public async Task Cannot_select_relationship_in_primary_resources()
        {
            // Arrange
            var article = new Article();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            var route = "/api/v1/articles?fields[articles]=author";

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
        public async Task Can_select_attribute_in_primary_resource_by_ID()
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
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            var route = $"/api/v1/articles/{article.StringId}?fields[articles]=url";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["url"].Should().Be(article.Url);
            responseDocument.SingleData.Relationships.Should().BeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Url.Should().Be(article.Url);
            articleCaptured.Caption.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_select_fields_of_HasOne_relationship()
        {
            // Arrange
            var article = new Article();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            var route = $"/api/v1/articles/{article.StringId}?fields[authors]=lastName,businessEmail,livingAddress";

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
        public async Task Cannot_select_fields_of_HasMany_relationship()
        {
            // Arrange
            var author = new Author();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Author>().InsertOneAsync(author);
            });

            var route = $"/api/v1/authors/{author.StringId}?fields[articles]=caption,tags";

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
        public async Task Cannot_select_fields_of_HasManyThrough_relationship()
        {
            // Arrange
            var article = new Article();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<Article>().InsertOneAsync(article);
            });

            var route = $"/api/v1/articles/{article.StringId}?fields[tags]=color";

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
                await db.ClearCollectionAsync<Article>();
                await db.GetCollection<Article>().InsertOneAsync(article);
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
        public async Task Retrieves_all_properties_when_fieldset_contains_readonly_attribute()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var todoItem = new TodoItem
            {
                Description = "Pending work..."
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<TodoItem>().InsertOneAsync(todoItem);
            });

            var route = $"/api/v1/todoItems/{todoItem.StringId}?fields[todoItems]=calculatedValue";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(todoItem.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["calculatedValue"].Should().Be(todoItem.CalculatedValue);
            responseDocument.SingleData.Relationships.Should().BeNull();

            var todoItemCaptured = (TodoItem) store.Resources.Should().ContainSingle(x => x is TodoItem).And.Subject.Single();
            todoItemCaptured.CalculatedValue.Should().Be(todoItem.CalculatedValue);
            todoItemCaptured.Description.Should().Be(todoItem.Description);
        }
    }
}
