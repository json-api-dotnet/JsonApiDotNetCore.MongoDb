using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly]
internal sealed partial class ContainerTypeToHideFromAutoDiscovery
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class LyricRepository : MongoRepository<Lyric, string?>, IAsyncDisposable
    {
        private readonly IOperationsTransaction _transaction;

        public override string TransactionId => _transaction.TransactionId;

        public LyricRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
            : base(mongoDataAccess, targetedFields, resourceGraph, resourceFactory, constraintProviders, resourceDefinitionAccessor)
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
