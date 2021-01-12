using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.MongoDb.Errors;
using JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace JsonApiDotNetCore.MongoDb.Repositories
{
    /// <summary>
    /// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses MongoDB.
    /// </summary>
    public class MongoDbRepository<TResource, TId> : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceFactory _resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;

        public MongoDbRepository(
            IMongoDatabase mongoDatabase,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));

            if (typeof(TId) != typeof(string))
            {
                throw new InvalidConfigurationException("MongoDB can only be used for resources with an 'Id' property of type 'string'.");
            }
        }

        protected virtual IMongoCollection<TResource> Collection => _mongoDatabase.GetCollection<TResource>(typeof(TResource).Name);

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer,
            CancellationToken cancellationToken)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            var query = ApplyQueryLayer(layer);
            var resources = await query.ToListAsync(cancellationToken);
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

            var queryExpressionValidator = new MongoDbQueryExpressionValidator();
            queryExpressionValidator.Validate(layer);

            AssertNoRelationshipsInSparseFieldSets();
            
            var source = GetAll();
            
            var queryableHandlers = _constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Where(expressionInScope => expressionInScope.Scope == null)
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<QueryableHandlerExpression>()
                .ToArray();

            foreach (var queryableHandler in queryableHandlers)
            {
                source = queryableHandler.Apply(source);
            }

            var nameFactory = new LambdaParameterNameFactory();
            var builder = new MongoDbQueryableBuilder(
                source.Expression,
                source.ElementType,
                typeof(Queryable),
                nameFactory,
                _resourceFactory,
                _resourceContextProvider,
                new MongoDbModel(_resourceContextProvider));

            var expression = builder.ApplyQuery(layer);
            return (IMongoQueryable<TResource>)source.Provider.CreateQuery<TResource>(expression);
        }
        
        protected virtual IQueryable<TResource> GetAll()
        {
            return Collection.AsQueryable();
        }
        
        private void AssertNoRelationshipsInSparseFieldSets()
        {
            var resourceContext = _resourceContextProvider.GetResourceContext<TResource>();

            var hasRelationshipSelectors = _constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<SparseFieldTableExpression>()
                .Any(fieldTable =>
                    fieldTable.Table.Keys.Any(targetResourceContext => targetResourceContext != resourceContext) ||
                    fieldTable.Table.Values.Any(fieldSet => fieldSet.Fields.Any(field => field is RelationshipAttribute)));

            if (hasRelationshipSelectors)
            {
                throw new UnsupportedRelationshipException();
            }
        }

        /// <inheritdoc />
        public virtual Task<TResource> GetForCreateAsync(TId id, CancellationToken cancellationToken)
        {
            var resource = _resourceFactory.CreateInstance<TResource>();
            resource.Id = id;

            return Task.FromResult(resource);
        }

        /// <inheritdoc />
        public virtual async Task CreateAsync(TResource resourceFromRequest, TResource resourceForDatabase,
            CancellationToken cancellationToken)
        {
            if (resourceFromRequest == null) throw new ArgumentNullException(nameof(resourceFromRequest));
            if (resourceForDatabase == null) throw new ArgumentNullException(nameof(resourceForDatabase));
            
            AssertNoRelationshipsAreTargeted();
            
            foreach (var attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceForDatabase, attribute.GetValue(resourceFromRequest));
            }

            try
            {
                await Collection.InsertOneAsync(resourceForDatabase, new InsertOneOptions(), cancellationToken);
            }
            catch (MongoWriteException ex)
            {
                throw new DataStoreUpdateException(ex);
            }
        }

        private void AssertNoRelationshipsAreTargeted()
        {
            if (_targetedFields.Relationships.Any())
            {
                throw new UnsupportedRelationshipException();
            }
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
            
            AssertNoRelationshipsAreTargeted();
            
            foreach (var attr in _targetedFields.Attributes)
            {
                attr.SetValue(resourceFromDatabase, attr.GetValue(resourceFromRequest));
            }

            var filter = Builders<TResource>.Filter.Eq(e => e.Id, resourceFromDatabase.Id);

            try
            {
                await Collection.ReplaceOneAsync(filter, resourceFromDatabase, new ReplaceOptions(), cancellationToken);
            }
            catch (MongoWriteException ex)
            {
                throw new DataStoreUpdateException(ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            var filter = Builders<TResource>.Filter.Eq(e => e.Id, id);

            DeleteResult result;
            try
            {
                result = await Collection.DeleteOneAsync(filter, new DeleteOptions(), cancellationToken);
            }
            catch (MongoWriteException ex)
            {
                throw new DataStoreUpdateException(ex);
            }

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
            throw new UnsupportedRelationshipException();
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
    public class MongoDbRepository<TResource> : MongoDbRepository<TResource, int>, IResourceRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public MongoDbRepository(
            IMongoDatabase mongoDatabase,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders)
            : base(mongoDatabase, targetedFields, resourceContextProvider, resourceFactory, constraintProviders)
        {
        }
    }
}
