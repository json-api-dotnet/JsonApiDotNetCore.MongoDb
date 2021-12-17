using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MetaDbContext : MongoDbContextShim
{
    public MongoDbSetShim<SupportTicket> SupportTickets => Set<SupportTicket>();

    public MetaDbContext(IMongoDatabase database)
        : base(database)
    {
    }
}
