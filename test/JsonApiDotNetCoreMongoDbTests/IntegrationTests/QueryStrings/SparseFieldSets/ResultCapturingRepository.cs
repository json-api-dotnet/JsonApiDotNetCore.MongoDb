using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.SparseFieldSets;

/// <summary>
/// Enables sparse fieldset tests to verify which fields were (not) retrieved from the database.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ResultCapturingRepository<TResource, TId> : MongoRepository<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly ResourceCaptureStore _captureStore;

    public ResultCapturingRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceGraph resourceGraph,
        IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IQueryableBuilder queryableBuilder, ResourceCaptureStore captureStore)
        : base(mongoDataAccess, targetedFields, resourceGraph, resourceFactory, constraintProviders, resourceDefinitionAccessor, queryableBuilder)
    {
        _captureStore = captureStore;
    }

    public override async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TResource> resources = await base.GetAsync(queryLayer, cancellationToken);

        _captureStore.Add(resources);

        return resources;
    }
}
