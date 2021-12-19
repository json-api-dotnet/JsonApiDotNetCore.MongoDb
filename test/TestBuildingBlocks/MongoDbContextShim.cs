using JsonApiDotNetCore.MongoDb.Resources;
using MongoDB.Driver;

namespace TestBuildingBlocks;

/// <summary>
/// Provides an Entity Framework Core DbContext-like abstraction that translates to MongoDB calls. This makes it easier to keep tests in sync with the
/// main repository.
/// </summary>
public abstract class MongoDbContextShim
{
    private readonly IMongoDatabase _database;
    private readonly List<MongoDbSetShim> _dbSetShims = new();

    protected MongoDbContextShim(IMongoDatabase database)
    {
        _database = database;
    }

    protected MongoDbSetShim<TEntity> Set<TEntity>()
        where TEntity : MongoIdentifiable
    {
        IMongoCollection<TEntity> collection = _database.GetCollection<TEntity>(typeof(TEntity).Name);
        var dbSetShim = new MongoDbSetShim<TEntity>(collection);

        _dbSetShims.Add(dbSetShim);
        return dbSetShim;
    }

    public async Task ClearTableAsync<TEntity>()
        where TEntity : MongoIdentifiable
    {
        await _database.DropCollectionAsync(typeof(TEntity).Name);
    }

    public async Task SaveChangesAsync(CancellationToken cancellation = default)
    {
        foreach (MongoDbSetShim dbSetShim in _dbSetShims)
        {
            await dbSetShim.PersistAsync(cancellation);
        }
    }
}
