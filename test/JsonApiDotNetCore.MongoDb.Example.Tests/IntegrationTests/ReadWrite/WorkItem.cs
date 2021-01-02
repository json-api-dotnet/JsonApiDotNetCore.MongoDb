using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

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
        
        [HasOne]
        [BsonIgnore]
        public UserAccount Assignee { get; set; }
        public MongoDBRef AssigneeId => new MongoDBRef(nameof(UserAccount), Assignee.Id);
        
        [HasMany]
        [BsonIgnore]
        public ISet<UserAccount> Subscribers { get; set; }
        public ISet<MongoDBRef> SubscriberIds => Subscribers
            .Select(sub => new MongoDBRef(nameof(UserAccount), sub.Id))
            .ToHashSet();

        [BsonIgnore]
        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set { }
        }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
