using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public sealed class Address : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }

        [Attr]
        public string Street { get; set; }

        [Attr]
        public string ZipCode { get; set; }

        [HasOne]
        public Country Country { get; set; }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
