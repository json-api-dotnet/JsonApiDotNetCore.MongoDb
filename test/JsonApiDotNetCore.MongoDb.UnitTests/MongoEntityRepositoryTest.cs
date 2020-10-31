using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Data;
using JsonApiDotNetCore.MongoDb.UnitTests.Models;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.UnitTests
{
    public sealed class MongoEntityRepositoryTests : IAsyncLifetime
    {
        private IResourceRepository<Book, string> Repository { get; set; }
        private IResourceGraph ResourceGraph { get; set; }
        private IMongoDatabase Database { get; set; }

        private IMongoCollection<Book> Books => Database.GetCollection<Book>(nameof(Book));

        private Mock<ITargetedFields> TargetedFields { get; set; }

        public MongoEntityRepositoryTests()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("JsonApiDotNet_MongoDb_Test");

            var targetedFields = new Mock<ITargetedFields>();
            targetedFields.Setup(tf => tf.Attributes).Returns(new List<AttrAttribute>());
            var resourceGraph = BuildGraph();
            var resourceFactory = new Mock<IResourceFactory>();
            var constraintProviders = new List<IQueryConstraintProvider>();

            TargetedFields = targetedFields;
            ResourceGraph = resourceGraph;
            Repository = new MongoEntityRepository<Book, string>(
                Database,
                targetedFields.Object,
                resourceGraph,
                resourceFactory.Object,
                constraintProviders);
        }

        private IResourceGraph BuildGraph()
        {
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            resourceGraphBuilder.Add<Book, string>();
            return resourceGraphBuilder.Build();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await Books.DeleteManyAsync(Builders<Book>.Filter.Empty);

        [Fact]
        public async Task ShouldCountZero()
        {
            var result = await Repository.CountAsync(
                new ComparisonExpression(
                    ComparisonOperator.Equals,
                    new LiteralConstantExpression(bool.FalseString),
                    new LiteralConstantExpression(bool.FalseString)));

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task ShouldCountThree()
        {
            for (var i = 0; i < 3; i++)
            {
                var book = new Book
                {
                    Name = $"Book {i + 1}",
                    Author = $"Author {i + 1}",
                    Category = $"Cat {i + 1}",
                    Price = 14.99M,
                };

                await Books.InsertOneAsync(book);
            }

            var result = await Repository.CountAsync(
                new ComparisonExpression(
                    ComparisonOperator.Equals,
                    new LiteralConstantExpression(bool.FalseString),
                    new LiteralConstantExpression(bool.FalseString)));

            Assert.Equal(3, result);
        }

        [Fact]
        public async Task ShouldSaveDocument()
        {
            var book = new Book
            {
                Name = "Harry Potter and the Deathly Hallows",
                Author = "JK Rowling",
                Category = "Adventure",
                Price = 14.99M,
            };

            await Repository.CreateAsync(book);

            var query = Books.AsQueryable();
            var saved = await query.FirstOrDefaultAsync();

            Assert.Equal(book.Name, saved.Name);
        }

        [Fact]
        public async Task ShouldGenerateObjectIdForNewDocument()
        {
            var book = new Book
            {
                Name = "Harry Potter and the Deathly Hallows",
                Author = "JK Rowling",
                Category = "Adventure",
                Price = 14.99M,
            };

            await Repository.CreateAsync(book);

            var query = Books.AsQueryable();
            var saved = await query.FirstOrDefaultAsync();

            Assert.NotNull(saved.Id);
        }

        [Fact]
        public async Task ShouldReturnTrue()
        {
            var book = new Book
            {
                Name = "Basic Philosophy",
                Author = "Some boring philosopher",
                Category = "Philosophy",
                Price = 2.00M,
            };

            await Books.InsertOneAsync(book);

            var query = Books.AsQueryable();
            var saved = await query.FirstOrDefaultAsync();

            var result = await Repository.DeleteAsync(saved.Id);
            Assert.True(result);
        }
        
        [Fact]
        public async Task ShouldDeleteDocument()
        {
            var book = new Book
            {
                Name = "Basic Philosophy",
                Author = "Some boring philosopher",
                Category = "Philosophy",
                Price = 2.00M,
            };

            await Books.InsertOneAsync(book);

            var query = Books.AsQueryable();
            var saved = await query.FirstOrDefaultAsync();

            await Repository.DeleteAsync(saved.Id);

            Assert.False(query.Any(b => b.Id == saved.Id));
        }

        [Fact]
        public async Task ShouldReturnFalse()
        {
            var result = await Repository.DeleteAsync("5f67c7718b884bb81fb0812e");
            Assert.False(result);
        }

        [Fact]
        public void ShouldThrowNotImplementedExceptionFlushFromCache()
        {
            // As far as I know MongoDB does not manage no cache
            // and therefore there is no cache to flush

            var book = new Book
            {
                Name = "Basic Philosophy",
                Author = "Some boring philosopher",
                Category = "Philosophy",
                Price = 2.00M,
            };

            Assert.Throws<NotImplementedException>(() =>
            {
                Repository.FlushFromCache(book);
            });
        }

        [Fact]
        public async Task ShouldReturnEmptyEnumerable()
        {
            var resourceContext = ResourceGraph.GetResourceContext<Book>();
            var result = await Repository.GetAsync(new QueryLayer(resourceContext));

            Assert.True(result.Count == 0);
        }

        [Fact]
        public async Task ShouldReturnThreeBooks()
        {
            for (var i = 0; i < 3; i++)
            {
                var book = new Book
                {
                    Name = $"Book {i + 1}",
                    Author = $"Author {i + 1}",
                    Category = $"Cat {i + 1}",
                    Price = 14.99M,
                };

                await Books.InsertOneAsync(book);
            }

            var resourceContext = ResourceGraph.GetResourceContext<Book>();
            var result = await Repository.GetAsync(new QueryLayer(resourceContext));

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task ShouldNotUpdate()
        {
            var book = new Book
            {
                Name = "Basic Philosophy",
                Author = "Some boring philosopher",
                Category = "Philosophy",
                Price = 2.00M,
            };

            await Books.InsertOneAsync(book);

            var query = Books.AsQueryable();
            var oldDoc = await query.FirstOrDefaultAsync();

            var newDoc = new Book
            {
                Name = "Basic Philosophy",
                Author = "Some boring philosopher",
                Category = "Philosophy",
                Price = 5.00M,
            };

            await Repository.UpdateAsync(newDoc, oldDoc);

            var saved = await query.FirstOrDefaultAsync();
            Assert.Equal(book.Price, saved.Price);
        }

        [Fact]
        public async Task ShouldUpdateBookPrice()
        {
            // TODO: Rewrite this test
            // This test is not done properly, it can fail at times
            // It is not independent of the context it's run on

            TargetedFields.Setup(tf => tf.Attributes).Returns(BookAttributes());

            var book = new Book
            {
                Name = "Basic Philosophy",
                Author = "Some boring philosopher",
                Category = "Philosophy",
                Price = 2.00M,
            };

            await Books.InsertOneAsync(book);

            var newDoc = new Book
            {
                Name = "Basic Philosophy",
                Author = "Some boring philosopher",
                Category = "Philosophy",
                Price = 5.00M,
            };

            await Repository.UpdateAsync(newDoc, book);

            var saved = await Books.AsQueryable().FirstOrDefaultAsync();
            Assert.Equal(5.00M, saved.Price);
        }

        private IList<AttrAttribute> BookAttributes()
        {
            var priceAttr = new AttrAttribute
            {
                PublicName = "price"
            };

            typeof(AttrAttribute)
                .GetProperty(nameof(AttrAttribute.Property))
                .SetValue(priceAttr, typeof(Book).GetProperty(nameof(Book.Price)));            

            return new List<AttrAttribute> { priceAttr };
        }

        [Fact]
        public async Task ShouldThrowNotImplementedExceptionUpdateRelationships()
        {
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await Repository.UpdateRelationshipAsync(null, null, null);
            });
        }
    }
}
