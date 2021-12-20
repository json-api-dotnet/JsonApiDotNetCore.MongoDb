using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading")]
public sealed class Planet : HexStringMongoIdentifiable
{
    [Attr]
    public string PublicName { get; set; } = null!;

    [Attr]
    public string? PrivateName { get; set; }

    [Attr]
    public bool HasRingSystem { get; set; }

    [Attr]
    public decimal SolarMass { get; set; }

    [HasMany]
    [BsonIgnore]
    public ISet<Moon> Moons { get; set; } = new HashSet<Moon>();
}
