using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite")]
public sealed class RgbColor : MongoIdentifiable
{
    [Attr]
    public string DisplayName { get; set; } = null!;

    [HasOne]
    [BsonIgnore]
    public WorkItemGroup? Group { get; set; }
}
