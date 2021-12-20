using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations")]
public sealed class RecordCompany : HexStringMongoIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? CountryOfResidence { get; set; }

    [HasMany]
    [BsonIgnore]
    public IList<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();

    [HasOne]
    [BsonIgnore]
    public RecordCompany? Parent { get; set; }
}
