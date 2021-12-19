using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite")]
public sealed class UserAccount : MongoIdentifiable
{
    [Attr]
    public string FirstName { get; set; } = null!;

    [Attr]
    public string LastName { get; set; } = null!;

    [HasMany]
    [BsonIgnore]
    public ISet<WorkItem> AssignedItems { get; set; } = new HashSet<WorkItem>();
}
