using MongoDB.Driver;

namespace JsonApiDotNetCore.MongoDb.Repositories;

/// <inheritdoc />
public sealed class MongoDataAccess : IMongoDataAccess
{
    /// <inheritdoc />
    public IMongoDatabase MongoDatabase { get; }

    /// <inheritdoc />
    public IClientSessionHandle? ActiveSession { get; set; }

    /// <inheritdoc />
    public string? TransactionId => ActiveSession is { IsInTransaction: true } ? ActiveSession.GetHashCode().ToString() : null;

    public MongoDataAccess(IMongoDatabase mongoDatabase)
    {
        ArgumentGuard.NotNull(mongoDatabase);

        MongoDatabase = mongoDatabase;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (ActiveSession != null)
        {
            if (ActiveSession.IsInTransaction)
            {
                await ActiveSession.AbortTransactionAsync();
            }

            ActiveSession.Dispose();
            ActiveSession = null;
        }
    }
}
