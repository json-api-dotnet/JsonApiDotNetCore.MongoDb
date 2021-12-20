using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class WorkTag : HexStringMongoIdentifiable
{
    [Attr]
    public string Text { get; set; } = null!;

    [Attr]
    public bool IsBuiltIn { get; set; }

    [HasMany]
    [BsonIgnore]
    public ISet<WorkItem> WorkItems { get; set; } = new HashSet<WorkItem>();
}
