using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MetaDbContext(IMongoDatabase database) : MongoDbContextShim(database)
{
    public MongoDbSetShim<SupportTicket> SupportTickets => Set<SupportTicket>();
}
