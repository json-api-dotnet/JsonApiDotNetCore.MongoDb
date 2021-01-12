using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.SparseFieldSets
{
    /// <summary>
    /// Enables sparse fieldset tests to verify which fields were (not) retrieved from the database.
    /// </summary>
    public sealed class ResultCapturingRepository<TResource> : MongoDbRepository<TResource, string>
        where TResource : class, IIdentifiable<string>
    {
        private readonly ResourceCaptureStore _captureStore;

        public ResultCapturingRepository(
            IMongoDatabase db,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            ResourceCaptureStore captureStore)
            : base(db, targetedFields, resourceContextProvider, resourceFactory, constraintProviders)
        {
            _captureStore = captureStore;
        }

        public override async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
        {
            var resources = await base.GetAsync(layer, cancellationToken);

            _captureStore.Add(resources);

            return resources;
        }
    }
}
