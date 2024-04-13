using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

/// <summary>
/// Used to simulate side effects that occur in the database while saving, typically caused by database triggers.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ImplicitlyChangingWorkItemGroupDefinition(IResourceGraph resourceGraph, ReadWriteDbContext dbContext)
    : JsonApiResourceDefinition<WorkItemGroup, string?>(resourceGraph)
{
    internal const string Suffix = " (changed)";

    private readonly ReadWriteDbContext _dbContext = dbContext;

    public override Task OnWriteSucceededAsync(WorkItemGroup resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (writeOperation is not WriteOperationKind.DeleteResource)
        {
            return _dbContext.Groups.ExecuteAsync(async collection =>
            {
                resource.Name += Suffix;

                FilterDefinition<WorkItemGroup> filter = Builders<WorkItemGroup>.Filter.Eq(group => group.Id, resource.Id);
                await collection.ReplaceOneAsync(filter, resource, cancellationToken: cancellationToken);
            });
        }

        return Task.CompletedTask;
    }
}
