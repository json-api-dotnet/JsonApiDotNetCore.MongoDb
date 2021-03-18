using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCore.MongoDb.AtomicOperations
{
    /// <summary>
    /// Provides transaction support for atomic:operation requests using MongoDB.
    /// </summary>
    public sealed class MongoTransactionFactory : IOperationsTransactionFactory
    {
        private readonly IMongoDataAccess _mongoDataAccess;

        public MongoTransactionFactory(IMongoDataAccess mongoDataAccess)
        {
            ArgumentGuard.NotNull(mongoDataAccess, nameof(mongoDataAccess));

            _mongoDataAccess = mongoDataAccess;
        }

        /// <inheritdoc />
        public async Task<IOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            bool transactionCreated = await CreateOrJoinTransactionAsync(cancellationToken);
            return new MongoTransaction(_mongoDataAccess, transactionCreated);
        }

        private async Task<bool> CreateOrJoinTransactionAsync(CancellationToken cancellationToken)
        {
            _mongoDataAccess.ActiveSession ??= await _mongoDataAccess.MongoDatabase.Client.StartSessionAsync(cancellationToken: cancellationToken);

            if (_mongoDataAccess.ActiveSession.IsInTransaction)
            {
                return false;
            }

            _mongoDataAccess.ActiveSession.StartTransaction();
            return true;
        }
    }
}
