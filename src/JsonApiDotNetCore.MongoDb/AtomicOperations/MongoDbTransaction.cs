using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCore.MongoDb.AtomicOperations
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class MongoDbTransaction : IOperationsTransaction
    {
        private readonly IMongoDataAccess _mongoDataAccess;
        private readonly bool _ownsTransaction;

        /// <inheritdoc />
        public string TransactionId => _mongoDataAccess.TransactionId;

        public MongoDbTransaction(IMongoDataAccess mongoDataAccess, bool ownsTransaction)
        {
            ArgumentGuard.NotNull(mongoDataAccess, nameof(mongoDataAccess));

            _mongoDataAccess = mongoDataAccess;
            _ownsTransaction = ownsTransaction;
        }

        /// <inheritdoc />
        public Task BeforeProcessOperationAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task AfterProcessOperationAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (_ownsTransaction)
            {
                await _mongoDataAccess.ActiveSession.CommitTransactionAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_ownsTransaction)
            {
                await _mongoDataAccess.DisposeAsync();
            }
        }
    }
}
