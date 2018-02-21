namespace JsonApiDotNetCore.MongoDb.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JsonApiDotNetCore.Data;
    using JsonApiDotNetCore.Extensions;
    using JsonApiDotNetCore.Internal.Query;
    using JsonApiDotNetCore.Models;
    using JsonApiDotNetCore.MongoDb.Extensions;
    using JsonApiDotNetCore.Services;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class MongoEntityRepository<TEntity, TId>
        : IEntityRepository<TEntity, TId>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly IMongoDatabase db;

        private readonly string collectionName;

        private readonly IJsonApiContext jsonApiContext;

        public MongoEntityRepository(IMongoDatabase db, string collectionName, IJsonApiContext jsonApiContext)
        {
            this.db = db;
            this.collectionName = collectionName;
            this.jsonApiContext = jsonApiContext;
        }

        private IMongoCollection<TEntity> Collection => this.db.GetCollection<TEntity>(this.collectionName);

        private IQueryable<TEntity> Entities => this.Collection.AsQueryable();

        public async Task<int> CountAsync(IQueryable<TEntity> entities)
        {
            return (int)await this.Collection.CountAsync(Builders<TEntity>.Filter.Empty);
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await this.Collection.InsertOneAsync(entity);

            return entity;
        }

        public async Task<bool> DeleteAsync(TId id)
        {
            var result = await this.Collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(e => e.Id, id));

            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public IQueryable<TEntity> Filter(IQueryable<TEntity> entities, FilterQuery filterQuery)
        {
            return entities.Filter(this.jsonApiContext, filterQuery);
        }

        public Task<TEntity> FirstOrDefaultAsync(IQueryable<TEntity> entities)
        {
            return entities.FirstOrDefaultAsync();
        }

        public IQueryable<TEntity> Get()
        {
            List<string> fields = this.jsonApiContext.QuerySet?.Fields;
            if (fields?.Any() ?? false)
            {
                return this.Entities.Select(fields);
            }

            return this.Entities;
        }

        public Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName)
        {
            // this is a document DB, no relations!
            return this.GetAsync(id);
        }

        public Task<TEntity> GetAsync(TId id)
        {
            return this.Collection.Find(Builders<TEntity>.Filter.Eq(e => e.Id, id)).SingleOrDefaultAsync();
        }

        public IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName)
        {
            // this is a document DB, no relations!
            return entities;
        }

        public async Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber)
        {
            return await entities.PageForward(pageSize, pageNumber).ToListAsync();
        }

        public Task<TEntity> SingleOrDefaultAsync(IQueryable<TEntity> queryable)
        {
            return queryable.SingleOrDefaultAsync();
        }

        public IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries)
        {
            return entities.Sort(sortQueries);
        }

        public Task<IReadOnlyList<TEntity>> ToListAsync(IQueryable<TEntity> entities)
        {
            return entities.ToListAsync();
        }

        public async Task<TEntity> UpdateAsync(TId id, TEntity entity)
        {
            var existingEntity = await this.GetAsync(id);

            if (existingEntity == null)
            {
                return null;
            }

            foreach (var attr in this.jsonApiContext.AttributesToUpdate)
            {
                attr.Key.SetValue(existingEntity, attr.Value);
            }

            foreach (var relationship in this.jsonApiContext.RelationshipsToUpdate)
            {
                relationship.Key.SetValue(existingEntity, relationship.Value);
            }

            await this.Collection.ReplaceOneAsync(Builders<TEntity>.Filter.Eq(e => e.Id, id), existingEntity);

            return existingEntity;
        }

        public Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TEntity> GetAllDocuments() => this.Collection.Find(new BsonDocument()).ToEnumerable();
    }
}
