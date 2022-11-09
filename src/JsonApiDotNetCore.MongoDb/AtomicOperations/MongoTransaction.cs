using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCore.MongoDb.AtomicOperations;

/// <inheritdoc />
[PublicAPI]
public sealed class MongoTransaction : IOperationsTransaction
{
    private readonly IMongoDataAccess _mongoDataAccess;
    private readonly bool _ownsTransaction;

    /// <inheritdoc />
    public string TransactionId => _mongoDataAccess.TransactionId!;

    public MongoTransaction(IMongoDataAccess mongoDataAccess, bool ownsTransaction)
    {
        ArgumentGuard.NotNull(mongoDataAccess);

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
        if (_ownsTransaction && _mongoDataAccess.ActiveSession != null)
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
