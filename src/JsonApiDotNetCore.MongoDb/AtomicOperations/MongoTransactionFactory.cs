using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCore.MongoDb.AtomicOperations;

/// <summary>
/// Provides transaction support for atomic:operation requests using MongoDB.
/// </summary>
public sealed class MongoTransactionFactory : IOperationsTransactionFactory
{
    private readonly IMongoDataAccess _mongoDataAccess;

    public MongoTransactionFactory(IMongoDataAccess mongoDataAccess)
    {
        ArgumentNullException.ThrowIfNull(mongoDataAccess);

        _mongoDataAccess = mongoDataAccess;
    }

    /// <inheritdoc />
    public async Task<TResult> RunInTransactionAsync<TResult>(Func<IOperationsTransaction, Task<TResult>> asyncAction, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);

        _mongoDataAccess.ActiveSession ??= await _mongoDataAccess.MongoDatabase.Client.StartSessionAsync(cancellationToken: cancellationToken);

        if (_mongoDataAccess.ActiveSession.IsInTransaction)
        {
            // Participate in existing transaction; nested transactions are not supported.
            await using var transaction = new MongoTransaction(_mongoDataAccess, _mongoDataAccess.TransactionId!);
            return await asyncAction(transaction);
        }

        // Commits automatically if no exception is thrown, retrying on transient failures.
        return await _mongoDataAccess.ActiveSession.WithTransactionAsync(async (_, _) =>
        {
            await using var transaction = new MongoTransaction(_mongoDataAccess, _mongoDataAccess.TransactionId!);
            return await asyncAction(transaction);
        }, cancellationToken: cancellationToken);
    }
}
