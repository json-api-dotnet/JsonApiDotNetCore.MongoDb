using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    internal static class ContainerTypeToHideMusicTrackRepositoryFromAutoDiscovery
    {
        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class MusicTrackRepository : MongoDbRepository<MusicTrack, string>
        {
            public override string TransactionId => null;

            public MusicTrackRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceContextProvider resourceContextProvider,
                IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders)
                : base(mongoDataAccess, targetedFields, resourceContextProvider, resourceFactory, constraintProviders)
            {
            }
        }
    }
}
