using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Extensions;
using JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.MongoDb
{
    public class MongoEntityRepository<TResource, TId>
        : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IMongoDatabase _db;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceFactory _resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;

        public MongoEntityRepository(
            IMongoDatabase db,
            ITargetedFields targetedFields,
            IResourceGraph resourceGraph,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            _db = db;
            _targetedFields = targetedFields;
            _resourceGraph = resourceGraph;
            _resourceFactory = resourceFactory;
            _constraintProviders = constraintProviders;
        }

        private IMongoCollection<TResource> Collection => _db.GetCollection<TResource>(typeof(TResource).Name);
        private IMongoQueryable<TResource> Entities => Collection.AsQueryable();

        public virtual Task<int> CountAsync(FilterExpression topFilter)
        {
            var resourceContext = _resourceGraph.GetResourceContext<TResource>();
            var layer = new QueryLayer(resourceContext)
            {
                Filter = topFilter
            };

            var query = ApplyQueryLayer(layer);
            return query.CountAsync();
        }

        public virtual Task CreateAsync(TResource resource) =>
            Collection.InsertOneAsync(resource);

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var result = await Collection.DeleteOneAsync(Builders<TResource>.Filter.Eq(e => e.Id, id));
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public virtual void FlushFromCache(TResource resource) =>
            throw new NotImplementedException();

        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer) =>
            await ApplyQueryLayer(layer).ToListAsync();

        public virtual async Task UpdateAsync(TResource requestResource, TResource databaseResource)
        {
            foreach (var attr in _targetedFields.Attributes)
                attr.SetValue(databaseResource, attr.GetValue(requestResource));

            await Collection.ReplaceOneAsync(Builders<TResource>.Filter.Eq(e => e.Id, databaseResource.Id), databaseResource);
        }

        public virtual Task UpdateRelationshipAsync(object parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds) =>
            throw new NotImplementedException();

        protected virtual IMongoQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            layer = layer ?? throw new ArgumentNullException(nameof(layer));

            var source = Entities;

            var queryableHandlers = _constraintProviders
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
            var builder = new QueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, _resourceFactory, _resourceGraph);

            var expression = builder.ApplyQuery(layer);
            return (IMongoQueryable<TResource>)source.Provider.CreateQuery<TResource>(expression);
        }
    }
}
