using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.MongoDb.Extensions;
using JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.MongoDb.Data
{
    public class MongoEntityRepository<TResource, TId>
        : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IMongoDatabase db;
        private readonly ITargetedFields targetedFields;
        private readonly IResourceGraph resourceGraph;
        private readonly IResourceFactory resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> constraintProviders;

        public MongoEntityRepository(
            IMongoDatabase db,
            ITargetedFields targetedFields,
            IResourceGraph resourceGraph,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            this.db = db;
            this.targetedFields = targetedFields;
            this.resourceGraph = resourceGraph;
            this.resourceFactory = resourceFactory;
            this.constraintProviders = constraintProviders;
        }

        private IMongoCollection<TResource> Collection => db.GetCollection<TResource>(typeof(TResource).Name);
        private IMongoQueryable<TResource> Entities => this.Collection.AsQueryable();

        public virtual Task<int> CountAsync(FilterExpression topFilter)
        {
            var resourceContext = resourceGraph.GetResourceContext<TResource>();
            var layer = new QueryLayer(resourceContext)
            {
                Filter = topFilter
            };

            var query = (IMongoQueryable<TResource>)ApplyQueryLayer(layer);
            return query.CountAsync();
        }

        public virtual Task CreateAsync(TResource resource)
        {
            return Collection.InsertOneAsync(resource);
        }

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var result = await this.Collection.DeleteOneAsync(Builders<TResource>.Filter.Eq(e => e.Id, id));
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public virtual void FlushFromCache(TResource resource)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer)
        {
            IQueryable<TResource> query = ApplyQueryLayer(layer);
            return await query.ToListAsync();
        }

        public virtual async Task UpdateAsync(TResource requestResource, TResource databaseResource)
        {
            foreach (var attr in targetedFields.Attributes)
                attr.SetValue(databaseResource, attr.GetValue(requestResource));

            await Collection.ReplaceOneAsync(Builders<TResource>.Filter.Eq(e => e.Id, databaseResource.Id), databaseResource);
        }

        public virtual Task UpdateRelationshipAsync(object parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds)
        {
            throw new NotImplementedException();
        }

        protected virtual IMongoQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            IMongoQueryable<TResource> source = Entities;

            var queryableHandlers = constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Where(expressionInScope => expressionInScope.Scope == null)
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<QueryableHandlerExpression>()
                .ToArray();

            foreach (var queryableHandler in queryableHandlers)
            {
                source = (IMongoQueryable<TResource>)queryableHandler.Apply(source);
            }

            var nameFactory = new JsonApiDotNetCore.Queries.Internal.QueryableBuilding.LambdaParameterNameFactory();
            var builder = new QueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, resourceFactory, resourceGraph);

            var expression = builder.ApplyQuery(layer);
            return (IMongoQueryable<TResource>)source.Provider.CreateQuery<TResource>(expression);
        }
    }
}
