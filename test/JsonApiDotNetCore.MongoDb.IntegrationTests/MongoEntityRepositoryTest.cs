using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.IntegrationTests.Models;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.IntegrationTests
{
    public sealed class MongoEntityRepositoryTests : IAsyncLifetime
    {
        private readonly IResourceRepository<Book, string> _repository;
        private readonly IMongoDatabase _database;
        private readonly Mock<ITargetedFields> _targetedFields;

        private IMongoCollection<Book> Books => _database.GetCollection<Book>(nameof(Book));

        public MongoEntityRepositoryTests()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("JsonApiDotNet_MongoDb_Test");

            var targetedFields = new Mock<ITargetedFields>();
            targetedFields.Setup(tf => tf.Attributes).Returns(new List<AttrAttribute>());
            var resourceGraph = BuildGraph();
            var resourceFactory = new Mock<IResourceFactory>();
            var constraintProviders = new List<IQueryConstraintProvider>();

            _targetedFields = targetedFields;
            _repository = new MongoEntityRepository<Book, string>(
                _database,
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
        public async Task UpdateAsync_ShouldUpdateOnlySpecifiedAttributes()
        {
            _targetedFields.Setup(tf => tf.Attributes).Returns(BookAttributes_PriceOnly());

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
                Category = "Not Philosophy",
                Price = 5.00M,
            };

            await _repository.UpdateAsync(newDoc, book);
            var saved = await Books.AsQueryable().FirstOrDefaultAsync();
            
            Assert.NotEqual(newDoc.Category, saved.Category);
            Assert.Equal(newDoc.Price, saved.Price);
        }

        private IList<AttrAttribute> BookAttributes_PriceOnly()
        {
            var priceAttr = new AttrAttribute
            {
                PublicName = "price"
            };

            typeof(AttrAttribute)
                .GetProperty(nameof(AttrAttribute.Property))
                ?.SetValue(priceAttr, typeof(Book).GetProperty(nameof(Book.Price)));            

            return new List<AttrAttribute> { priceAttr };
        }

        [Fact]
        public void FlushFromCache_ShouldThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                _repository.FlushFromCache(null);
            });
        }

        [Fact]
        public async Task UpdateRelationshipAsync_ShouldThrowNotImplementedException()
        {
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                _repository.UpdateRelationshipAsync(null, null, null));
        }
    }
}
