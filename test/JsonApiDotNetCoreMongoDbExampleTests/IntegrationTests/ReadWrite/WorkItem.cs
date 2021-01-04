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
        
        [HasOne]
        [BsonIgnore]
        public UserAccount Assignee { get; set; }
        
        [HasMany]
        [BsonIgnore]
        public ISet<UserAccount> Subscribers { get; set; }

        [BsonIgnore]
        [Attr(Capabilities = ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set { }
        }
    }
}
