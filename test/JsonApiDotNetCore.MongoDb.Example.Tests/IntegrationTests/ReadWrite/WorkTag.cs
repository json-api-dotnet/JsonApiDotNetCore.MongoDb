using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    public sealed class WorkTag : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }

        [Attr]
        public string Text { get; set; }

        [Attr]
        public bool IsBuiltIn { get; set; }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
