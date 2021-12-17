using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UniverseDbContext : MongoDbContextShim
{
    public MongoDbSetShim<Star> Stars => Set<Star>();
    public MongoDbSetShim<Planet> Planets => Set<Planet>();
    public MongoDbSetShim<Moon> Moons => Set<Moon>();

    public UniverseDbContext(IMongoDatabase database)
        : base(database)
    {
    }
}
