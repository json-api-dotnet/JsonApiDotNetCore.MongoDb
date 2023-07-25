using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings")]
public sealed class Label : HexStringMongoIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public LabelColor Color { get; set; }

    [HasMany]
    [BsonIgnore]
    public ISet<BlogPost> Posts { get; set; } = new HashSet<BlogPost>();
}
