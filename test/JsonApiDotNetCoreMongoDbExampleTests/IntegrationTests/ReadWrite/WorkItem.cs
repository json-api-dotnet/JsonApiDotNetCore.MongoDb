using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItem : MongoIdentifiable
    {
        [Attr]
        public string Description { get; set; }

        [Attr]
        public DateTimeOffset? DueAt { get; set; }

        [Attr]
        public WorkItemPriority Priority { get; set; }

        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        [BsonIgnore]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set => _ = value;
        }

        [HasOne]
        [BsonIgnore]
        public UserAccount Assignee { get; set; }

        [HasMany]
        [BsonIgnore]
        public ISet<UserAccount> Subscribers { get; set; }

        [HasManyThrough(nameof(WorkItemTags))]
        [BsonIgnore]
        public ISet<WorkTag> Tags { get; set; }

        [BsonIgnore]
        public ICollection<WorkItemTag> WorkItemTags { get; set; }

        [HasOne]
        [BsonIgnore]
        public WorkItem Parent { get; set; }

        [HasMany]
        [BsonIgnore]
        public IList<WorkItem> Children { get; set; }

        [HasManyThrough(nameof(RelatedFromItems), LeftPropertyName = nameof(WorkItemToWorkItem.ToItem),
            RightPropertyName = nameof(WorkItemToWorkItem.FromItem))]
        [BsonIgnore]
        public IList<WorkItem> RelatedFrom { get; set; }

        [BsonIgnore]
        public IList<WorkItemToWorkItem> RelatedFromItems { get; set; }

        [HasManyThrough(nameof(RelatedToItems), LeftPropertyName = nameof(WorkItemToWorkItem.FromItem), RightPropertyName = nameof(WorkItemToWorkItem.ToItem))]
        [BsonIgnore]
        public IList<WorkItem> RelatedTo { get; set; }

        [BsonIgnore]
        public IList<WorkItemToWorkItem> RelatedToItems { get; set; }

        [HasOne]
        [BsonIgnore]
        public WorkItemGroup Group { get; set; }
    }
}
