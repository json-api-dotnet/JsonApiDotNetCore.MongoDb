using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ReadWriteDbContext : MongoDbContextShim
{
    public MongoDbSetShim<WorkItem> WorkItems => Set<WorkItem>();
    public MongoDbSetShim<WorkTag> WorkTags => Set<WorkTag>();
    public MongoDbSetShim<WorkItemGroup> Groups => Set<WorkItemGroup>();
    public MongoDbSetShim<RgbColor> RgbColors => Set<RgbColor>();
    public MongoDbSetShim<UserAccount> UserAccounts => Set<UserAccount>();

    public ReadWriteDbContext(IMongoDatabase database)
        : base(database)
    {
    }
}
