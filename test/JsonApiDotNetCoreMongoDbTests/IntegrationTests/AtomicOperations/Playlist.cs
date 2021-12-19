using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations")]
public sealed class Playlist : MongoIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    [BsonIgnore]
    public bool IsArchived => false;

    [HasMany]
    [BsonIgnore]
    public IList<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
}
