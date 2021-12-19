using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly]
internal sealed partial class ContainerTypeToHideFromAutoDiscovery
{
    /// <summary>
    /// Used to simulate side effects that occur in the database while saving, typically caused by database triggers.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ImplicitlyChangingWorkItemDefinition : JsonApiResourceDefinition<WorkItem, string?>
    {
        internal const string Suffix = " (changed)";

        private readonly ReadWriteDbContext _dbContext;

        public ImplicitlyChangingWorkItemDefinition(IResourceGraph resourceGraph, ReadWriteDbContext dbContext)
            : base(resourceGraph)
        {
            _dbContext = dbContext;
        }

        public override async Task OnWriteSucceededAsync(WorkItem resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                await _dbContext.WorkItems.ExecuteAsync(async collection =>
                {
                    resource.Description += Suffix;

                    FilterDefinition<WorkItem> filter = Builders<WorkItem>.Filter.Eq(item => item.Id, resource.Id);
                    await collection.ReplaceOneAsync(filter, resource, cancellationToken: cancellationToken);
                });
            }
        }
    }
}
