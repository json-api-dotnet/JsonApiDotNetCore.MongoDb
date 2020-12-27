using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Extensions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace JsonApiDotNetCore.MongoDb
{
    public class MongoEntityRepository<TResource, TId>
        : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IMongoDatabase _db;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceFactory _resourceFactory;
        
        public MongoEntityRepository(
            IMongoDatabase db,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory)
        {
            _db = db;
            _targetedFields = targetedFields;
            _resourceContextProvider = resourceContextProvider;
            _resourceFactory = resourceFactory;
        }

        private IMongoCollection<TResource> Collection => _db.GetCollection<TResource>(typeof(TResource).Name);
        private IMongoQueryable<TResource> Entities => Collection.AsQueryable();

        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken) =>
            await ApplyQueryLayer(layer).ToListAsync();

        public virtual Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            var layer = new QueryLayer(resourceContext)
            {
                Filter = topFilter
            };

            var query = ApplyQueryLayer(layer);
            return query.CountAsync(cancellationToken);
        }

        public virtual Task<TResource> GetForCreateAsync(TId id, CancellationToken cancellationToken)
        {
            var resource = _resourceFactory.CreateInstance<TResource>();
            resource.Id = id;

            return Task.FromResult(resource);
        }

        public virtual Task CreateAsync(TResource resourceFromRequest, TResource resourceForDatabase,
            CancellationToken cancellationToken)
        {
            if (resourceFromRequest == null) throw new ArgumentNullException(nameof(resourceFromRequest));
            if (resourceForDatabase == null) throw new ArgumentNullException(nameof(resourceForDatabase));
            
            foreach (var attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceForDatabase, attribute.GetValue(resourceFromRequest));
            }

            return Collection.InsertOneAsync(resourceForDatabase, new InsertOneOptions(), cancellationToken);
        }

        public virtual async Task<TResource> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            var resources = await GetAsync(queryLayer, cancellationToken);
            return resources.FirstOrDefault();
        }

        public virtual async Task UpdateAsync(TResource requestResource, TResource databaseResource, CancellationToken cancellationToken)
        {
            foreach (var attr in _targetedFields.Attributes)
                attr.SetValue(databaseResource, attr.GetValue(requestResource));

            await Collection.ReplaceOneAsync(
                Builders<TResource>.Filter.Eq(e => e.Id, databaseResource.Id),
                databaseResource,
                new ReplaceOptions(),
                cancellationToken);
        }

        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            var result = await Collection.DeleteOneAsync(
                Builders<TResource>.Filter.Eq(e => e.Id, id),
                new DeleteOptions(),
                cancellationToken);

            if (!result.IsAcknowledged || result.DeletedCount == 0)
            {
                throw new DataStoreUpdateException(new Exception());
            }
        }

        public virtual Task SetRelationshipAsync(TResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task AddToToManyRelationshipAsync(TId primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task RemoveFromToManyRelationshipAsync(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected virtual IMongoQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            var source = Entities;

            var nameFactory = new LambdaParameterNameFactory();
            var builder = new QueryableBuilder(
                source.Expression,
                source.ElementType,
                typeof(Queryable),
                nameFactory,
                _resourceFactory,
                _resourceContextProvider,
                DummyModel.Instance);

            var expression = builder.ApplyQuery(layer);
            return (IMongoQueryable<TResource>)source.Provider.CreateQuery<TResource>(expression);
        }
    }

    internal sealed class DummyModel : IModel
    {
        public static IModel Instance { get; } = new DummyModel();

        private DummyModel()
        {
        }

        public IAnnotation FindAnnotation(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAnnotation> GetAnnotations()
        {
            throw new NotImplementedException();
        }

        public object this[string name] => throw new NotImplementedException();

        public IEnumerable<IEntityType> GetEntityTypes()
        {
            throw new NotImplementedException();
        }

        public IEntityType FindEntityType(string name)
        {
            throw new NotImplementedException();
        }

        public IEntityType FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
        {
            throw new NotImplementedException();
        }
    }
}
