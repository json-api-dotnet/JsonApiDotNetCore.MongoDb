using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings")]
public sealed class Blog : MongoIdentifiable
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public string PlatformName { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
    public bool ShowAdvertisements => PlatformName.EndsWith("(using free account)", StringComparison.Ordinal);

    [HasMany]
    [BsonIgnore]
    public IList<BlogPost> Posts { get; set; } = new List<BlogPost>();

    [HasOne]
    [BsonIgnore]
    public WebAccount? Owner { get; set; }
}
