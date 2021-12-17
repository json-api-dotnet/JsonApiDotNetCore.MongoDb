using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Transactions;

internal sealed partial class ContainerTypeToHideFromAutoDiscovery
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class MusicTrackRepository : MongoRepository<MusicTrack, string?>
    {
        public override string? TransactionId => null;

        public MusicTrackRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
            : base(mongoDataAccess, targetedFields, resourceGraph, resourceFactory, constraintProviders, resourceDefinitionAccessor)
        {
        }
    }
}
