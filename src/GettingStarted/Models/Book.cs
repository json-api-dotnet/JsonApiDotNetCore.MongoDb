using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GettingStarted.Models
{
    public sealed class Book : MongoDbIdentifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [Attr]
        public string Category { get; set; }

        [Attr]
        public string Author { get; set; }
    }
}
