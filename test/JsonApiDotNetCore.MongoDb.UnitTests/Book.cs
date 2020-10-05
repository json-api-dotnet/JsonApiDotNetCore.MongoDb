
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.UnitTests.Models
{
    public sealed class Book : Identifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public override string Id { get; set; }

        [Attr]
        public string Name { get; set; }

        [Attr]
        public decimal Price { get; set; }

        [Attr]
        public string Category { get; set; }

        [Attr]
        public string Author { get; set; }
    }
}
