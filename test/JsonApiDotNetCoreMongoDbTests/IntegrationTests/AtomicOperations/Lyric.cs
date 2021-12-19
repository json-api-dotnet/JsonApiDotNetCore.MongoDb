using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations")]
public sealed class Lyric : MongoIdentifiable
{
    [Attr]
    public string? Format { get; set; }

    [Attr]
    public string Text { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.None)]
    public DateTimeOffset CreatedAt { get; set; }

    [HasOne]
    [BsonIgnore]
    public TextLanguage? Language { get; set; }

    [HasOne]
    [BsonIgnore]
    public MusicTrack? Track { get; set; }
}
