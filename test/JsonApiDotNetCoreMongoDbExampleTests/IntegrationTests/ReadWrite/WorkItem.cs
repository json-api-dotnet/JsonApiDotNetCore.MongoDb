using System;
using System.Collections.Generic;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    public sealed class WorkItem : MongoDbIdentifiable
    {
        [Attr]
        public string Description { get; set; }

        [Attr]
        public DateTimeOffset? DueAt { get; set; }

        [Attr]
        public WorkItemPriority Priority { get; set; }

        [BsonIgnore]
        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set { }
        }

        [BsonIgnore]
        [HasOne]
        public UserAccount Assignee { get; set; }

        [BsonIgnore]
        [HasMany]
        public ISet<UserAccount> Subscribers { get; set; }

        [BsonIgnore]
        [HasManyThrough(nameof(WorkItemTags))]
        public ISet<WorkTag> Tags { get; set; }
        public ICollection<WorkItemTag> WorkItemTags { get; set; }

        [BsonIgnore]
        [HasOne]
        public WorkItem Parent { get; set; }

        [BsonIgnore]
        [HasMany]
        public IList<WorkItem> Children { get; set; }

        [BsonIgnore]
        [HasManyThrough(nameof(RelatedFromItems), LeftPropertyName = nameof(WorkItemToWorkItem.ToItem), RightPropertyName = nameof(WorkItemToWorkItem.FromItem))]
        public IList<WorkItem> RelatedFrom { get; set; }
        public IList<WorkItemToWorkItem> RelatedFromItems { get; set; }

        [BsonIgnore]
        [HasManyThrough(nameof(RelatedToItems), LeftPropertyName = nameof(WorkItemToWorkItem.FromItem), RightPropertyName = nameof(WorkItemToWorkItem.ToItem))]
        public IList<WorkItem> RelatedTo { get; set; }
        public IList<WorkItemToWorkItem> RelatedToItems { get; set; }

        [BsonIgnore]
        [HasOne]
        public WorkItemGroup Group { get; set; }
    }
}
