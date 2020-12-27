using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    public sealed class WorkItemGroup : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }

        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsPublic { get; set; }

        // [NotMapped]
        // [Attr]
        // public Guid ConcurrencyToken => Guid.NewGuid();

        // [HasOne]
        // public RgbColor Color { get; set; }

        // [HasMany]
        // public IList<WorkItem> Items { get; set; }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
