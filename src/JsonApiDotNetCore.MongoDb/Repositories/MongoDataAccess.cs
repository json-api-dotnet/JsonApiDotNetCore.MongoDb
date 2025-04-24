using Microsoft.EntityFrameworkCore.Metadata;
using MongoDB.Driver;

namespace JsonApiDotNetCore.MongoDb.Repositories;

/// <inheritdoc cref="IMongoDataAccess" />
public sealed class MongoDataAccess : IMongoDataAccess
{
    /// <inheritdoc />
    public IReadOnlyModel EntityModel { get; }

    /// <inheritdoc />
    public IMongoDatabase MongoDatabase { get; }

    /// <inheritdoc />
    public IClientSessionHandle? ActiveSession { get; set; }

    /// <inheritdoc />
    public string? TransactionId => ActiveSession is { IsInTransaction: true } ? ActiveSession.GetHashCode().ToString() : null;

    public MongoDataAccess(IReadOnlyModel entityModel, IMongoDatabase mongoDatabase)
    {
        ArgumentNullException.ThrowIfNull(entityModel);
        ArgumentNullException.ThrowIfNull(mongoDatabase);

        EntityModel = entityModel;
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
