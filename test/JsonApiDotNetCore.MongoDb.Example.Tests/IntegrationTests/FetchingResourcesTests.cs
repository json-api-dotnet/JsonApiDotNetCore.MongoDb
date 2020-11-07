using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using Example;
using Example.Models;
using Humanizer;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests
{
    public sealed class FetchingResourcesTests : IClassFixture<IntegrationTestContext<Startup>>, IAsyncLifetime
    {
        private readonly Faker<Book> _bookFaker;

        private readonly IntegrationTestContext<Startup> _testContext;
        private readonly IEnumerable<Book> _books;

        public FetchingResourcesTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            _bookFaker = new Faker<Book>()
                .RuleFor(b => b.Name, f => f.Lorem.Sentence())
                .RuleFor(b => b.Author, f => f.Name.FindName())
                .RuleFor(b => b.Category, f => f.Commerce.ProductAdjective())
                .RuleFor(b => b.Price, f => f.Random.Decimal(1.00M, 50.00M));

            _books = GenerateBooks().ToList();
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton(sp =>
                {
                    var client = new MongoClient("mongodb://localhost:27017");
                    return client.GetDatabase("JsonApiDotNetCore_MongoDb_Example_Tests");
                });
            });
        }
        
        private IEnumerable<Book> GenerateBooks()
        {
            for (var i = 0; i < 30; i++)
            {
                yield return _bookFaker.Generate();
            }
        }

        public Task InitializeAsync() => _testContext.RunOnDatabaseAsync(
            db => db.GetCollection<Book>(nameof(Book))
                .InsertManyAsync(_books));

        public Task DisposeAsync() => _testContext.RunOnDatabaseAsync(db => db.DropCollectionAsync(nameof(Book)));

        [Fact]
        public async Task ShouldGetAllResources()
        {
            var route = "/api/Books";
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);
            
            var expected = _books
                .Select(b => b.StringId)
                .ToArray();
            var actual = responseDocument.ManyData?.Select(x => x.Id).ToArray();
            
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.Equal(expected.Length, (Int64)responseDocument.Meta["totalResources"]);
            Assert.Equal(expected.Take(10).ToArray(), actual);
        }
        
        [Theory]
        [InlineData("12.99")]
        [InlineData("23.45")]
        [InlineData("12.00")]
        public async Task ShouldGetBooksWithPriceEqualTo(string priceStr)
        {
            var price = Convert.ToDecimal(priceStr);
            var route = $"/api/Books?filter=equals(price,'{price}')";
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            var expected = _books
                .Where(b => b.Price == price)
                .Take(10)
                .Select(b => b.StringId)
                .ToArray();
            var actual = responseDocument.ManyData?.Select(x => x.Id).ToArray();
            
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.Equal(expected.Length, (Int64)responseDocument.Meta["totalResources"]);
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData("12.99", "29.34")]
        [InlineData("23.45", "47.22")]
        [InlineData("2.00", "11.32")]
        public async Task ShouldGetBooksWithPriceBetween(string minPriceStr, string maxPriceStr)
        {
            var minPrice = Convert.ToDecimal(minPriceStr);
            var maxPrice = Convert.ToDecimal(maxPriceStr);
            
            var route = $"/api/Books?filter=and(greaterOrEqual(price,'{minPrice}'),lessOrEqual(price,'{maxPrice}'))";
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            var expected = _books
                .Where(b => b.Price >= minPrice && b.Price <= maxPrice)
                .Select(b => b.StringId)
                .ToArray();
            var actual = responseDocument.ManyData?.Select(x => x.Id).ToArray();
            
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.Equal(expected.Length, (Int64)responseDocument.Meta["totalResources"]);
            Assert.Equal(expected.Take(10).ToArray(), actual);
        }
        
        [Theory]
        [InlineData(3, 4)]
        [InlineData(10, 2)]
        [InlineData(20, 1)]
        public async Task ShouldPaginate(int pageSize, int pageNumber)
        {
            var route = $"/api/Books?page[size]={pageSize}&page[number]={pageNumber}";
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            var expected = _books
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => b.StringId)
                .ToArray();
            var actual = responseDocument.ManyData?.Select(x => x.Id).ToArray();
            
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData("name")]
        [InlineData("price")]
        public async Task ShouldSortByField(string field)
        {
            var route = $"/api/Books?sort={field}";
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            var expected = _books
                .OrderBy(b => b.GetType().GetProperty(field.Pascalize())?.GetValue(b))
                .Take(10)
                .Select(b => b.StringId)
                .ToArray();
            var actual = responseDocument.ManyData.Select(x => x.Id).ToArray();
            
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.Equal(expected, actual);
        }
    }
}
