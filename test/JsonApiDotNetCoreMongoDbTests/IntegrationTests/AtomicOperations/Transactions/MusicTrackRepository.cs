using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MusicTrackRepository(
    IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder)
    : MongoRepository<MusicTrack, string?>(mongoDataAccess, targetedFields, resourceGraph, resourceFactory, constraintProviders, resourceDefinitionAccessor,
        queryableBuilder)
{
    public override string? TransactionId => null;
}
