using System.Net;
using System.Threading.Tasks;
using Bogus;
using Example;
using Example.Models;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests
{
    public sealed class CreatingResourcesTests : IClassFixture<IntegrationTestContext<Startup>>, IAsyncLifetime
    {
        private readonly IntegrationTestContext<Startup> _testContext;
        private readonly Faker<Book> _bookFaker;

        private string _createdBookId;

        public CreatingResourcesTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;
            _bookFaker = new Faker<Book>()
                .RuleFor(b => b.Name, f => f.Lorem.Sentence())
                .RuleFor(b => b.Author, f => f.Name.FindName())
                .RuleFor(b => b.Category, f => f.Commerce.ProductAdjective())
                .RuleFor(b => b.Price, f => f.Random.Decimal(1.00M, 50.00M));
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton(sp =>
                {
                    var client = new MongoClient("mongodb://localhost:27017");
                    return client.GetDatabase("JsonApiDotNetCore_MongoDb_Resource_Creation_Tests");
                });
            });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => _testContext.RunOnDatabaseAsync(db => db.DropCollectionAsync(nameof(Book)));

        [Fact]
        public async Task ShouldCreateResource()
        {
            var route = "/api/Books";
            var book = _bookFaker.Generate();
            var resource = new
            {
                data = new
                {
                    type = "books",
                    attributes = new
                    {
                        name = book.Name,
                        price = book.Price,
                        category = book.Category,
                        author = book.Author
                    }   
                }
            };
            
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, resource);
            _createdBookId = responseDocument.Data is ResourceObject resourceObject ? resourceObject.Id : null;
                
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        }
    }
}
