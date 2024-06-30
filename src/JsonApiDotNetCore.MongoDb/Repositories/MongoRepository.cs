using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.MongoDb.Errors;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace JsonApiDotNetCore.MongoDb.Repositories;

/// <summary>
/// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses MongoDB.
/// </summary>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
[PublicAPI]
public class MongoRepository<TResource, TId> : IResourceRepository<TResource, TId>, IRepositorySupportsTransaction
    where TResource : class, IIdentifiable<TId>
{
    private readonly IMongoDataAccess _mongoDataAccess;
    private readonly ITargetedFields _targetedFields;
    private readonly IResourceGraph _resourceGraph;
    private readonly IResourceFactory _resourceFactory;
    private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly IQueryableBuilder _queryableBuilder;

    protected virtual IMongoCollection<TResource> Collection => _mongoDataAccess.MongoDatabase.GetCollection<TResource>(typeof(TResource).Name);

    /// <inheritdoc />
    public virtual string? TransactionId => _mongoDataAccess.TransactionId;

    public MongoRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
        IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder)
    {
        ArgumentGuard.NotNull(mongoDataAccess);
        ArgumentGuard.NotNull(targetedFields);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(resourceFactory);
        ArgumentGuard.NotNull(constraintProviders);
        ArgumentGuard.NotNull(resourceDefinitionAccessor);
        ArgumentGuard.NotNull(queryableBuilder);

        _mongoDataAccess = mongoDataAccess;
        _targetedFields = targetedFields;
        _resourceGraph = resourceGraph;
        _resourceFactory = resourceFactory;
        _constraintProviders = constraintProviders;
        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _queryableBuilder = queryableBuilder;

        if (!typeof(TResource).IsAssignableTo(typeof(IMongoIdentifiable)))
        {
            throw new InvalidConfigurationException("MongoDB can only be used with resources that implement 'IMongoIdentifiable'.");
        }
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(queryLayer);

        IMongoQueryable<TResource> query = ApplyQueryLayer(queryLayer);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<int> CountAsync(FilterExpression? topFilter, CancellationToken cancellationToken)
    {
        ResourceType resourceType = _resourceGraph.GetResourceType<TResource>();

        var layer = new QueryLayer(resourceType)
        {
            Filter = topFilter
        };

        IMongoQueryable<TResource> query = ApplyQueryLayer(layer);
        return query.CountAsync(cancellationToken);
    }

#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    protected virtual IMongoQueryable<TResource> ApplyQueryLayer(QueryLayer queryLayer)
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
    {
        ArgumentGuard.NotNull(queryLayer);

        var queryExpressionValidator = new MongoQueryExpressionValidator();
        queryExpressionValidator.Validate(queryLayer);

        AssertNoRelationshipsInSparseFieldSets();

        IQueryable<TResource> source = GetAll();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        QueryableHandlerExpression[] queryableHandlers = _constraintProviders
            .SelectMany(provider => provider.GetConstraints())
            .Where(expressionInScope => expressionInScope.Scope == null)
            .Select(expressionInScope => expressionInScope.Expression)
            .OfType<QueryableHandlerExpression>()
            .ToArray();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        foreach (QueryableHandlerExpression queryableHandler in queryableHandlers)
        {
            source = queryableHandler.Apply(source);
        }

        var context = QueryableBuilderContext.CreateRoot(source, typeof(Queryable), _mongoDataAccess.EntityModel, null);
        Expression expression = _queryableBuilder.ApplyQuery(queryLayer, context);

        return (IMongoQueryable<TResource>)source.Provider.CreateQuery<TResource>(expression);
    }

    protected virtual IQueryable<TResource> GetAll()
    {
        return _mongoDataAccess.ActiveSession != null ? Collection.AsQueryable(_mongoDataAccess.ActiveSession) : Collection.AsQueryable();
    }

    private void AssertNoRelationshipsInSparseFieldSets()
    {
        ResourceType resourceType = _resourceGraph.GetResourceType<TResource>();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        bool hasRelationshipSelectors = _constraintProviders
            .SelectMany(provider => provider.GetConstraints())
            .Select(expressionInScope => expressionInScope.Expression)
            .OfType<SparseFieldTableExpression>()
            .Any(fieldTable => fieldTable.Table.Keys.Any(targetResourceType => !resourceType.Equals(targetResourceType)) ||
                fieldTable.Table.Values.Any(fieldSet => fieldSet.Fields.Any(field => field is RelationshipAttribute)));

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        if (hasRelationshipSelectors)
        {
            throw new UnsupportedRelationshipException();
        }
    }

    /// <inheritdoc />
    public virtual Task<TResource> GetForCreateAsync(Type resourceClrType, [DisallowNull] TId id, CancellationToken cancellationToken)
    {
        var resource = (TResource)_resourceFactory.CreateInstance(resourceClrType);
        resource.Id = id;

        return Task.FromResult(resource);
    }

    /// <inheritdoc />
    public virtual async Task CreateAsync(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(resourceFromRequest);
        ArgumentGuard.NotNull(resourceForDatabase);

        AssertNoRelationshipsAreTargeted();

        foreach (AttrAttribute attribute in _targetedFields.Attributes)
        {
            attribute.SetValue(resourceForDatabase, attribute.GetValue(resourceFromRequest));
        }

        await _resourceDefinitionAccessor.OnWritingAsync(resourceForDatabase, WriteOperationKind.CreateResource, cancellationToken);

        await SaveChangesAsync(
            async () => await (_mongoDataAccess.ActiveSession != null
                ? Collection.InsertOneAsync(_mongoDataAccess.ActiveSession, resourceForDatabase, cancellationToken: cancellationToken)
                : Collection.InsertOneAsync(resourceForDatabase, cancellationToken: cancellationToken)), cancellationToken);

        await _resourceDefinitionAccessor.OnWriteSucceededAsync(resourceForDatabase, WriteOperationKind.CreateResource, cancellationToken);
    }

    private void AssertNoRelationshipsAreTargeted()
    {
        if (_targetedFields.Relationships.Any())
        {
            throw new UnsupportedRelationshipException();
        }
    }

    /// <inheritdoc />
    public virtual async Task<TResource?> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(queryLayer);

        IReadOnlyCollection<TResource> resources = await GetAsync(queryLayer, cancellationToken);
        return resources.FirstOrDefault();
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(resourceFromRequest);
        ArgumentGuard.NotNull(resourceFromDatabase);

        AssertNoRelationshipsAreTargeted();

        foreach (AttrAttribute attr in _targetedFields.Attributes)
        {
            attr.SetValue(resourceFromDatabase, attr.GetValue(resourceFromRequest));
        }

        await _resourceDefinitionAccessor.OnWritingAsync(resourceFromDatabase, WriteOperationKind.UpdateResource, cancellationToken);

        FilterDefinition<TResource> filter = Builders<TResource>.Filter.Eq(resource => resource.Id, resourceFromDatabase.Id);

        await SaveChangesAsync(
            () => _mongoDataAccess.ActiveSession != null
                ? Collection.ReplaceOneAsync(_mongoDataAccess.ActiveSession, filter, resourceFromDatabase, cancellationToken: cancellationToken)
                : Collection.ReplaceOneAsync(filter, resourceFromDatabase, cancellationToken: cancellationToken), cancellationToken);

        await _resourceDefinitionAccessor.OnWriteSucceededAsync(resourceFromDatabase, WriteOperationKind.UpdateResource, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TResource? resourceFromDatabase, [DisallowNull] TId id, CancellationToken cancellationToken)
    {
        TResource placeholderResource = resourceFromDatabase ?? _resourceFactory.CreateInstance<TResource>();
        placeholderResource.Id = id;

        await _resourceDefinitionAccessor.OnWritingAsync(placeholderResource, WriteOperationKind.DeleteResource, cancellationToken);

        FilterDefinition<TResource> filter = Builders<TResource>.Filter.Eq(resource => resource.Id, id);

        DeleteResult result = await SaveChangesAsync(
            () => _mongoDataAccess.ActiveSession != null
                ? Collection.DeleteOneAsync(_mongoDataAccess.ActiveSession, filter, cancellationToken: cancellationToken)
                : Collection.DeleteOneAsync(filter, cancellationToken), cancellationToken);

        if (!result.IsAcknowledged)
        {
            throw new DataStoreUpdateException(
                new Exception($"Failed to delete document with id '{id}', because the operation was not acknowledged by MongoDB."));
        }

        if (result.DeletedCount == 0)
        {
            throw new DataStoreUpdateException(new Exception($"Failed to delete document with id '{id}', because it does not exist."));
        }

        await _resourceDefinitionAccessor.OnWriteSucceededAsync(placeholderResource, WriteOperationKind.DeleteResource, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task SetRelationshipAsync(TResource leftResource, object? rightValue, CancellationToken cancellationToken)
    {
        throw new UnsupportedRelationshipException();
    }

    /// <inheritdoc />
    public virtual Task AddToToManyRelationshipAsync(TResource? leftResource, [DisallowNull] TId leftId, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        throw new UnsupportedRelationshipException();
    }

    /// <inheritdoc />
    public virtual Task RemoveFromToManyRelationshipAsync(TResource leftResource, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
    {
        throw new UnsupportedRelationshipException();
    }

    protected virtual async Task SaveChangesAsync(Func<Task> asyncSaveAction, CancellationToken cancellationToken)
    {
        _ = await SaveChangesAsync<object?>(async () =>
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
