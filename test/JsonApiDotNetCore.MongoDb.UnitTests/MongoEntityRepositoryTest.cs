using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Data;
using JsonApiDotNetCore.MongoDb.UnitTests.Models;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.MongoDb.UnitTests
{
    [TestClass]
    public class MongoEntityRepositoryTests
    {
        private IResourceRepository<Book, string> Repository { get; set; }
        private IMongoDatabase Database { get; set; }

        private IMongoCollection<Book> Books => Database.GetCollection<Book>(nameof(Book));

        [TestInitialize]
        public void BeforeEach()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("JsonApiDotNet_MongoDb_Test");

            var targetedFields = new Mock<ITargetedFields>();
            var resourceGraph = new Mock<IResourceGraph>();
            resourceGraph.Setup(g => g.GetResourceContext<Book>()).Returns(new Mock<ResourceContext>().Object);
            var resourceFactory = new Mock<IResourceFactory>();
            var constraintProviders = new List<IQueryConstraintProvider>();

            Repository = new MongoEntityRepository<Book, string>(
                Database,
                targetedFields.Object,
                resourceGraph.Object,
                resourceFactory.Object,
                constraintProviders);
        }

        [TestCleanup]
        public async Task AfterEach()
        {
            await Books.DeleteManyAsync(Builders<Book>.Filter.Empty);
        }

        [TestMethod]
        public async Task ShouldCountZero()
        {
            var result = await Repository.CountAsync(
                new ComparisonExpression(
                    ComparisonOperator.Equals,
                    new LiteralConstantExpression(bool.FalseString),
                    new LiteralConstantExpression(bool.FalseString)));

            Assert.AreEqual(0, result);
        }

        [TestMethod]
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

            Assert.AreEqual(3, result);
        }

        [TestMethod]
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

            Assert.AreEqual(book.Name, saved.Name);
        }

        [TestMethod]
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

            Assert.IsNotNull(saved.Id);
        }

        [TestMethod]
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
            Assert.IsTrue(result);
        }
        
        [TestMethod]
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

            Assert.IsFalse(query.Any(b => b.Id == saved.Id));
        }

        [TestMethod]
        public async Task ShouldReturnFalse()
        {
            var result = await Repository.DeleteAsync("5f67c7718b884bb81fb0812e");
            Assert.IsFalse(result);
        }
    }
}
