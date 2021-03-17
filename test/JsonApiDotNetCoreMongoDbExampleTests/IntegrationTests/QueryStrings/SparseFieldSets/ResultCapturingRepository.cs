using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings.SparseFieldSets
{
    /// <summary>
    /// Enables sparse fieldset tests to verify which fields were (not) retrieved from the database.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ResultCapturingRepository<TResource> : MongoDbRepository<TResource, string>
        where TResource : class, IIdentifiable<string>
    {
        private readonly ResourceCaptureStore _captureStore;

        public ResultCapturingRepository(IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ResourceCaptureStore captureStore)
            : base(mongoDataAccess, targetedFields, resourceContextProvider, resourceFactory, constraintProviders)
        {
            _captureStore = captureStore;
        }

        public override async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<TResource> resources = await base.GetAsync(layer, cancellationToken);

            _captureStore.Add(resources);

            return resources;
        }
    }
}
