using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCore.MongoDb.AtomicOperations;

/// <inheritdoc cref="IOperationsTransaction" />
[PublicAPI]
public sealed class MongoTransaction : IOperationsTransaction
{
    /// <inheritdoc />
    public string TransactionId { get; }

    public MongoTransaction(IMongoDataAccess mongoDataAccess, string transactionId)
    {
        ArgumentNullException.ThrowIfNull(transactionId);

        TransactionId = transactionId;
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
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
