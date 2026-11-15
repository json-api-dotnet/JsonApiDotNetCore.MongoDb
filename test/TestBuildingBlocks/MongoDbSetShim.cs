using System.Linq.Expressions;
using JsonApiDotNetCore.MongoDb.Resources;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace TestBuildingBlocks;

public abstract class MongoDbSetShim
{
    internal abstract Task PersistAsync();
}

/// <summary>
/// Provides an Entity Framework Core DbSet-like abstraction that translates to MongoDB calls. This makes it easier to keep tests in sync with the main
/// repository.
/// </summary>
public sealed class MongoDbSetShim<TEntity> : MongoDbSetShim
    where TEntity : IMongoIdentifiable
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly List<TEntity> _entitiesToInsert = [];

    internal MongoDbSetShim(IMongoCollection<TEntity> collection)
    {
        _collection = collection;
    }

    public void Add(TEntity entity)
    {
        _entitiesToInsert.Add(entity);
    }

    public void AddRange(params TEntity[] entities)
    {
        _entitiesToInsert.AddRange(entities);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _entitiesToInsert.AddRange(entities);
    }

    internal override async Task PersistAsync()
    {
        if (_entitiesToInsert.Count > 0)
        {
            if (_entitiesToInsert.Count == 1)
            {
                await _collection.InsertOneAsync(_entitiesToInsert[0], cancellationToken: TestContext.Current.CancellationToken);
            }
            else
            {
                await _collection.InsertManyAsync(_entitiesToInsert, cancellationToken: TestContext.Current.CancellationToken);
            }

            _entitiesToInsert.Clear();
        }
    }

    public async Task ExecuteAsync(Func<IMongoCollection<TEntity>, Task> action)
    {
        await action(_collection);
    }

    public async Task<TEntity> FirstWithIdAsync(string? id)
    {
        TEntity entity = await _collection.AsQueryable().FirstOrDefaultAsync(document => Equals(document.Id, id), TestContext.Current.CancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"Resource with ID '{id}' was not found.");
        }

        return entity;
    }

    public async Task<TEntity?> FirstWithIdOrDefaultAsync(string? id)
    {
        return await _collection.AsQueryable().FirstOrDefaultAsync(document => Equals(document.Id, id), TestContext.Current.CancellationToken);
    }

    public async Task<List<TEntity>> ToListAsync()
    {
        return await _collection.AsQueryable().ToListAsync(TestContext.Current.CancellationToken);
    }

    public async Task<List<TEntity>> ToListWhereAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _collection.AsQueryable().Where(predicate).ToListAsync(TestContext.Current.CancellationToken);
    }
}
