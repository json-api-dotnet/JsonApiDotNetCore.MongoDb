using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FilterDbContext : MongoDbContextShim
{
    public MongoDbSetShim<FilterableResource> FilterableResources => Set<FilterableResource>();

    public FilterDbContext(IMongoDatabase database)
        : base(database)
    {
    }
}
