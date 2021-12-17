using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Transactions;

internal static class ContainerTypeToHideLyricRepositoryFromAutoDiscovery
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class LyricRepository : MongoRepository<Lyric, string>, IAsyncDisposable
    {
        private readonly IOperationsTransaction _transaction;

        public override string TransactionId => _transaction.TransactionId;

        public LyricRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders)
            : base(mongoDataAccess, targetedFields, resourceContextProvider, resourceFactory, constraintProviders)
        {
            IMongoDataAccess otherDataAccess = new MongoDataAccess(mongoDataAccess.MongoDatabase);

            var factory = new MongoTransactionFactory(otherDataAccess);
            _transaction = factory.BeginTransactionAsync(CancellationToken.None).Result;
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.DisposeAsync();
        }
    }
}
