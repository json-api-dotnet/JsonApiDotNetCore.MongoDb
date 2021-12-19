using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

internal sealed partial class ContainerTypeToHideFromAutoDiscovery
{
    /// <summary>
    /// Used to simulate side effects that occur in the database while saving, typically caused by database triggers.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ImplicitlyChangingWorkItemGroupDefinition : JsonApiResourceDefinition<WorkItemGroup, string?>
    {
        internal const string Suffix = " (changed)";

        private readonly ReadWriteDbContext _dbContext;

        public ImplicitlyChangingWorkItemGroupDefinition(IResourceGraph resourceGraph, ReadWriteDbContext dbContext)
            : base(resourceGraph)
        {
            _dbContext = dbContext;
        }

        public override async Task OnWriteSucceededAsync(WorkItemGroup resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                await _dbContext.Groups.ExecuteAsync(async collection =>
                {
                    resource.Name += Suffix;

                    FilterDefinition<WorkItemGroup> filter = Builders<WorkItemGroup>.Filter.Eq(group => group.Id, resource.Id);
                    await collection.ReplaceOneAsync(filter, resource, cancellationToken: cancellationToken);
                });
            }
        }
    }
}
