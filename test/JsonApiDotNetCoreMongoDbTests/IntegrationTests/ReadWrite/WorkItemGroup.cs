using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite")]
public sealed class WorkItemGroup : MongoIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public bool IsPublic { get; set; }

    [Attr]
    [BsonIgnore]
    public bool IsDeprecated => !string.IsNullOrEmpty(Name) && Name.StartsWith("DEPRECATED:", StringComparison.OrdinalIgnoreCase);

    [HasOne]
    [BsonIgnore]
    public RgbColor? Color { get; set; }

    [HasMany]
    [BsonIgnore]
    public IList<WorkItem> Items { get; set; } = new List<WorkItem>();
}
