using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace JsonApiDotNetCore.MongoDb
{
    /// <summary>
    /// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses MongoDB.
    /// </summary>
    public class MongoDbRepository<TResource, TId>
        : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceFactory _resourceFactory;
        
        public MongoDbRepository(
            IMongoDatabase mongoDatabase,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        }

        protected virtual IMongoCollection<TResource> Collection => _mongoDatabase.GetCollection<TResource>(typeof(TResource).Name);

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer,
            CancellationToken cancellationToken)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            
            var resources = await ApplyQueryLayer(layer).ToListAsync(cancellationToken);
            return resources.AsReadOnly();
        }

        /// <inheritdoc />
        public virtual Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            var layer = new QueryLayer(resourceContext)
            {
                Filter = topFilter
            };

            var query = ApplyQueryLayer(layer);
            return query.CountAsync(cancellationToken);
        }
        
        protected virtual IMongoQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            
            var source = GetAll();

            var nameFactory = new LambdaParameterNameFactory();
            var builder = new QueryableBuilder(
                source.Expression,
                source.ElementType,
                typeof(Queryable),
                nameFactory,
                _resourceFactory,
                _resourceContextProvider,
                DummyModel.Instance);

            var expression = builder.ApplyQuery(layer);
            return (IMongoQueryable<TResource>)source.Provider.CreateQuery<TResource>(expression);
        }
        
        protected virtual IQueryable<TResource> GetAll()
        {
            return Collection.AsQueryable();
        }

        /// <inheritdoc />
        public virtual Task<TResource> GetForCreateAsync(TId id, CancellationToken cancellationToken)
        {
            var resource = _resourceFactory.CreateInstance<TResource>();
            resource.Id = id;

            return Task.FromResult(resource);
        }

        /// <inheritdoc />
        public virtual Task CreateAsync(TResource resourceFromRequest, TResource resourceForDatabase,
            CancellationToken cancellationToken)
        {
            if (resourceFromRequest == null) throw new ArgumentNullException(nameof(resourceFromRequest));
            if (resourceForDatabase == null) throw new ArgumentNullException(nameof(resourceForDatabase));
            
            foreach (var attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceForDatabase, attribute.GetValue(resourceFromRequest));
            }

            return Collection.InsertOneAsync(resourceForDatabase, new InsertOneOptions(), cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            var resources = await GetAsync(queryLayer, cancellationToken);
            return resources.FirstOrDefault();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
        {
            if (resourceFromRequest == null) throw new ArgumentNullException(nameof(resourceFromRequest));
            if (resourceFromDatabase == null) throw new ArgumentNullException(nameof(resourceFromDatabase));
            
            foreach (var attr in _targetedFields.Attributes)
                attr.SetValue(resourceFromDatabase, attr.GetValue(resourceFromRequest));

            await Collection.ReplaceOneAsync(
                Builders<TResource>.Filter.Eq(e => e.Id, resourceFromDatabase.Id),
                resourceFromDatabase,
                new ReplaceOptions(),
                cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            var result = await Collection.DeleteOneAsync(
                Builders<TResource>.Filter.Eq(e => e.Id, id),
                new DeleteOptions(),
                cancellationToken);

            if (!result.IsAcknowledged)
            {
                throw new DataStoreUpdateException(new Exception($"Failed to delete document with id '{id}', because the operation was not acknowledged by MongoDB."));
            }

            if (result.DeletedCount == 0)
            {
                throw new DataStoreUpdateException(new Exception($"Failed to delete document with id '{id}', because it does not exist."));
            }
        }

        /// <inheritdoc />
        public virtual Task SetRelationshipAsync(TResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Task AddToToManyRelationshipAsync(TId primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Task RemoveFromToManyRelationshipAsync(TResource primaryResource,
            ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses MongoDB.
    /// </summary>
    public class MongoDbRepository<TResource> : MongoDbRepository<TResource, string>
        where TResource : class, IIdentifiable<string>
    {
        public MongoDbRepository(
            IMongoDatabase mongoDatabase,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory)
            : base(mongoDatabase, targetedFields, resourceContextProvider, resourceFactory)
        {
        }
    }

    internal sealed class DummyModel : IModel
    {
        public static IModel Instance { get; } = new DummyModel();

        public object this[string name] => throw new NotImplementedException();
        
        private DummyModel()
        {
        }

        public IAnnotation FindAnnotation(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAnnotation> GetAnnotations()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEntityType> GetEntityTypes()
        {
            throw new NotImplementedException();
        }

        public IEntityType FindEntityType(string name)
        {
            throw new NotImplementedException();
        }

        public IEntityType FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
        {
            throw new NotImplementedException();
        }
    }
}
