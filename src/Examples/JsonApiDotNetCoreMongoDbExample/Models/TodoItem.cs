using System;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public class TodoItem : MongoDbIdentifiable, IIsLockable
    {
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
    }
}
