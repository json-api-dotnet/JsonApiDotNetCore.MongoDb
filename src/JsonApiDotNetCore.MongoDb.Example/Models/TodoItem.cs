using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public class TodoItem : IIdentifiable<string>, IIsLockable
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        public bool IsLocked { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public long Ordinal { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowCreate)]
        public string AlwaysChangingValue
        {
            get => Guid.NewGuid().ToString();
            set { }
        }

        [Attr]
        public DateTime CreatedDate { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
        public DateTime? AchievedDate { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public string CalculatedValue => "calculated";

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowChange)]
        public DateTimeOffset? OffsetDate { get; set; }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
