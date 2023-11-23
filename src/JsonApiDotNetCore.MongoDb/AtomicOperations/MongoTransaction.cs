using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCore.MongoDb.AtomicOperations;

/// <inheritdoc cref="IOperationsTransaction" />
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
    public Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_ownsTransaction && _mongoDataAccess.ActiveSession != null)
        {
            return _mongoDataAccess.ActiveSession.CommitTransactionAsync(cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_ownsTransaction)
        {
            return _mongoDataAccess.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }
}
