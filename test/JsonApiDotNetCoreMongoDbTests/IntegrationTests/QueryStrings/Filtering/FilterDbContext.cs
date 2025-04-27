using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FilterDbContext(IMongoDatabase database)
    : MongoDbContextShim(database)
{
    public MongoDbSetShim<FilterableResource> FilterableResources => Set<FilterableResource>();
}
