using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Transactions;

internal sealed partial class ContainerTypeToHideFromAutoDiscovery
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class PerformerRepository : IResourceRepository<Performer, string?>
    {
        public Task<IReadOnlyCollection<Performer>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(FilterExpression? filter, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Performer> GetForCreateAsync(string? id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(Performer resourceFromRequest, Performer resourceForDatabase, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Performer?> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Performer resourceFromRequest, Performer resourceFromDatabase, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string? id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRelationshipAsync(Performer leftResource, object? rightValue, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddToToManyRelationshipAsync(string? leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromToManyRelationshipAsync(Performer leftResource, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
