using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite")]
public sealed class WorkItem : MongoIdentifiable
{
    [Attr]
    public string? Description { get; set; }

    [Attr]
    public DateTimeOffset? DueAt { get; set; }

    [Attr]
    public WorkItemPriority Priority { get; set; }

    [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
    [BsonIgnore]
    public bool IsImportant
    {
        get => Priority == WorkItemPriority.High;
        set => Priority = value ? WorkItemPriority.High : throw new NotSupportedException();
    }

    [HasOne]
    [BsonIgnore]
    public UserAccount? Assignee { get; set; }

    [HasMany]
    [BsonIgnore]
    public ISet<UserAccount> Subscribers { get; set; } = new HashSet<UserAccount>();

    [HasMany]
    [BsonIgnore]
    public ISet<WorkTag> Tags { get; set; } = new HashSet<WorkTag>();

    [HasOne]
    [BsonIgnore]
    public WorkItem? Parent { get; set; }

    [HasMany]
    [BsonIgnore]
    public IList<WorkItem> Children { get; set; } = new List<WorkItem>();

    [HasMany]
    [BsonIgnore]
    public IList<WorkItem> RelatedFrom { get; set; } = new List<WorkItem>();

    [HasMany]
    [BsonIgnore]
    public IList<WorkItem> RelatedTo { get; set; } = new List<WorkItem>();

    [HasOne]
    [BsonIgnore]
    public WorkItemGroup? Group { get; set; }
}
