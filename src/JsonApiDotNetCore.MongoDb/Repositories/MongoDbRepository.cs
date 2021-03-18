using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
    [PublicAPI]
    public class MongoDbRepository<TResource, TId> : IResourceRepository<TResource, TId>, IRepositorySupportsTransaction
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IMongoDataAccess _mongoDataAccess;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceFactory _resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;

        protected virtual IMongoCollection<TResource> Collection => _mongoDataAccess.MongoDatabase.GetCollection<TResource>(typeof(TResource).Name);

        /// <inheritdoc />
        public virtual string TransactionId => _mongoDataAccess.TransactionId;

        public MongoDbRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            ArgumentGuard.NotNull(mongoDataAccess, nameof(mongoDataAccess));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));

            _mongoDataAccess = mongoDataAccess;
            _targetedFields = targetedFields;
            _resourceContextProvider = resourceContextProvider;
            _resourceFactory = resourceFactory;
            _constraintProviders = constraintProviders;

            if (typeof(TId) != typeof(string))
            {
                throw new InvalidConfigurationException("MongoDB can only be used for resources with an 'Id' property of type 'string'.");
            }
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(layer, nameof(layer));

            IMongoQueryable<TResource> query = ApplyQueryLayer(layer);
            List<TResource> resources = await query.ToListAsync(cancellationToken);
            return resources.AsReadOnly();
        }

        /// <inheritdoc />
        public virtual Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
        {
            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext<TResource>();

            var layer = new QueryLayer(resourceContext)
            {
                Filter = topFilter
            };

            IMongoQueryable<TResource> query = ApplyQueryLayer(layer);
            return query.CountAsync(cancellationToken);
        }

        protected virtual IMongoQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            ArgumentGuard.NotNull(layer, nameof(layer));

            var queryExpressionValidator = new MongoDbQueryExpressionValidator();
            queryExpressionValidator.Validate(layer);

            AssertNoRelationshipsInSparseFieldSets();

            IQueryable<TResource> source = GetAll();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            QueryableHandlerExpression[] queryableHandlers = _constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Where(expressionInScope => expressionInScope.Scope == null)
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<QueryableHandlerExpression>()
                .ToArray();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            foreach (QueryableHandlerExpression queryableHandler in queryableHandlers)
            {
                source = queryableHandler.Apply(source);
            }

            var nameFactory = new LambdaParameterNameFactory();

            var builder = new MongoDbQueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, _resourceFactory,
                _resourceContextProvider, new MongoDbModel(_resourceContextProvider));

            Expression expression = builder.ApplyQuery(layer);
            return (IMongoQueryable<TResource>)source.Provider.CreateQuery<TResource>(expression);
        }

        protected virtual IQueryable<TResource> GetAll()
        {
            return _mongoDataAccess.ActiveSession != null ? Collection.AsQueryable(_mongoDataAccess.ActiveSession) : Collection.AsQueryable();
        }

        private void AssertNoRelationshipsInSparseFieldSets()
        {
            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext<TResource>();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            bool hasRelationshipSelectors = _constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<SparseFieldTableExpression>()
                .Any(fieldTable =>
                    fieldTable.Table.Keys.Any(targetResourceContext => targetResourceContext != resourceContext) ||
                    fieldTable.Table.Values.Any(fieldSet => fieldSet.Fields.Any(field => field is RelationshipAttribute)));

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

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
        public virtual async Task CreateAsync(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(resourceFromRequest, nameof(resourceFromRequest));
            ArgumentGuard.NotNull(resourceForDatabase, nameof(resourceForDatabase));

            AssertNoRelationshipsAreTargeted();

            foreach (AttrAttribute attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceForDatabase, attribute.GetValue(resourceFromRequest));
            }

            await SaveChangesAsync(async () =>
            {
                await (_mongoDataAccess.ActiveSession != null
                    ? Collection.InsertOneAsync(_mongoDataAccess.ActiveSession, resourceForDatabase, cancellationToken: cancellationToken)
                    : Collection.InsertOneAsync(resourceForDatabase, cancellationToken: cancellationToken));
            }, cancellationToken);
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
            IReadOnlyCollection<TResource> resources = await GetAsync(queryLayer, cancellationToken);
            return resources.FirstOrDefault();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(resourceFromRequest, nameof(resourceFromRequest));
            ArgumentGuard.NotNull(resourceFromDatabase, nameof(resourceFromDatabase));

            AssertNoRelationshipsAreTargeted();

            foreach (AttrAttribute attr in _targetedFields.Attributes)
            {
                attr.SetValue(resourceFromDatabase, attr.GetValue(resourceFromRequest));
            }

            FilterDefinition<TResource> filter = Builders<TResource>.Filter.Eq(resource => resource.Id, resourceFromDatabase.Id);

            await SaveChangesAsync(async () =>
            {
                await (_mongoDataAccess.ActiveSession != null
                    ? Collection.ReplaceOneAsync(_mongoDataAccess.ActiveSession, filter, resourceFromDatabase, cancellationToken: cancellationToken)
                    : Collection.ReplaceOneAsync(filter, resourceFromDatabase, cancellationToken: cancellationToken));
            }, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            FilterDefinition<TResource> filter = Builders<TResource>.Filter.Eq(resource => resource.Id, id);

            DeleteResult result = await SaveChangesAsync(
                async () => _mongoDataAccess.ActiveSession != null
                    ? await Collection.DeleteOneAsync(_mongoDataAccess.ActiveSession, filter, cancellationToken: cancellationToken)
                    : await Collection.DeleteOneAsync(filter, cancellationToken), cancellationToken);

            if (!result.IsAcknowledged)
            {
                throw new DataStoreUpdateException(
                    new Exception($"Failed to delete document with id '{id}', because the operation was not acknowledged by MongoDB."));
            }

            if (result.DeletedCount == 0)
            {
                throw new DataStoreUpdateException(new Exception($"Failed to delete document with id '{id}', because it does not exist."));
            }
        }

        /// <inheritdoc />
        public virtual Task SetRelationshipAsync(TResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new UnsupportedRelationshipException();
        }

        /// <inheritdoc />
        public virtual Task AddToToManyRelationshipAsync(TId primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new UnsupportedRelationshipException();
        }

        /// <inheritdoc />
        public virtual Task RemoveFromToManyRelationshipAsync(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            throw new UnsupportedRelationshipException();
        }

        protected virtual async Task SaveChangesAsync(Func<Task> asyncSaveAction, CancellationToken cancellationToken)
        {
            _ = await SaveChangesAsync<object>(async () =>
            {
                await asyncSaveAction();
                return null;
            }, cancellationToken);
        }

        protected virtual async Task<TResult> SaveChangesAsync<TResult>(Func<Task<TResult>> asyncSaveAction, CancellationToken cancellationToken)
        {
            try
            {
                return await asyncSaveAction();
            }
            catch (MongoException exception)
            {
                if (_mongoDataAccess.ActiveSession != null)
                {
                    // The ResourceService calling us needs to run additional SQL queries after an aborted transaction,
                    // to determine error cause. This fails when a failed transaction is still in progress.
                    await _mongoDataAccess.ActiveSession.AbortTransactionAsync(cancellationToken);
                    _mongoDataAccess.ActiveSession = null;
                }

                throw new DataStoreUpdateException(exception);
            }
        }
    }

    /// <summary>
    /// Do not use. This type exists solely to produce a proper error message when trying to use MongoDB with a non-string Id.
    /// </summary>
    public sealed class MongoDbRepository<TResource> : MongoDbRepository<TResource, int>, IResourceRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public MongoDbRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders)
            : base(mongoDataAccess, targetedFields, resourceContextProvider, resourceFactory, constraintProviders)
        {
        }
    }
}
