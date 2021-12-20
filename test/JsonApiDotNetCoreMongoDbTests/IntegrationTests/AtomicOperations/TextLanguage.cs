using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations")]
public sealed class TextLanguage : FreeStringMongoIdentifiable
{
    [Attr]
    public string? IsoCode { get; set; }

    [Attr(Capabilities = AttrCapabilities.None)]
    public bool IsRightToLeft { get; set; }

    [HasMany]
    [BsonIgnore]
    public ICollection<Lyric> Lyrics { get; set; } = new List<Lyric>();
}
