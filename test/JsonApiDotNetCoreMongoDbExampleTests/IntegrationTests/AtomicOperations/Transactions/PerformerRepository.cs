using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    internal static class ContainerTypeToHidePerformerRepositoryFromAutoDiscovery
    {
        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class PerformerRepository : IResourceRepository<Performer, string>
        {
            public Task<IReadOnlyCollection<Performer>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<Performer> GetForCreateAsync(string id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task CreateAsync(Performer resourceFromRequest, Performer resourceForDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<Performer> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task UpdateAsync(Performer resourceFromRequest, Performer resourceFromDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(string id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SetRelationshipAsync(Performer primaryResource, object secondaryResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(string primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RemoveFromToManyRelationshipAsync(Performer primaryResource, ISet<IIdentifiable> secondaryResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
