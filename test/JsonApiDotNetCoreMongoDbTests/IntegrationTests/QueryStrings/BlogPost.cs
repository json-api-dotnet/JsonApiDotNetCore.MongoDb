using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings")]
public sealed class BlogPost : HexStringMongoIdentifiable
{
    [Attr]
    public string Caption { get; set; } = null!;

    [Attr]
    public string Url { get; set; } = null!;

    [HasOne]
    [BsonIgnore]
    public WebAccount? Author { get; set; }

    [HasOne]
    [BsonIgnore]
    public WebAccount? Reviewer { get; set; }

    [HasMany]
    [BsonIgnore]
    public ISet<Label> Labels { get; set; } = new HashSet<Label>();

    [HasMany]
    [BsonIgnore]
    public ISet<Comment> Comments { get; set; } = new HashSet<Comment>();

    [HasOne(Capabilities = HasOneCapabilities.All & ~HasOneCapabilities.AllowInclude)]
    [BsonIgnore]
    public Blog? Parent { get; set; }
}
