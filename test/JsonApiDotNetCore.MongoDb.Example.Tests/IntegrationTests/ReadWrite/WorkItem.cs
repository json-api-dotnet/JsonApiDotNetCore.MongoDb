using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    public sealed class WorkItem : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }

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

        // [HasOne]
        // public UserAccount Assignee { get; set; }

        // [HasMany]
        // public ISet<UserAccount> Subscribers { get; set; }

        // [NotMapped]
        // [HasManyThrough(nameof(WorkItemTags))]
        // public ISet<WorkTag> Tags { get; set; }
        // public ICollection<WorkItemTag> WorkItemTags { get; set; }

        // [HasOne]
        // public WorkItem Parent { get; set; }

        // [HasMany]
        // public IList<WorkItem> Children { get; set; }

        // [NotMapped]
        // [HasManyThrough(nameof(RelatedFromItems), LeftPropertyName = nameof(WorkItemToWorkItem.ToItem), RightPropertyName = nameof(WorkItemToWorkItem.FromItem))]
        // public IList<WorkItem> RelatedFrom { get; set; }
        // public IList<WorkItemToWorkItem> RelatedFromItems { get; set; }

        // [NotMapped]
        // [HasManyThrough(nameof(RelatedToItems), LeftPropertyName = nameof(WorkItemToWorkItem.FromItem), RightPropertyName = nameof(WorkItemToWorkItem.ToItem))]
        // public IList<WorkItem> RelatedTo { get; set; }
        // public IList<WorkItemToWorkItem> RelatedToItems { get; set; }

        // [HasOne]
        // public WorkItemGroup Group { get; set; }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
