using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.Helpers.Models
{
    public class Movie : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr] public string Name { get; set; }
        
        [HasOne]
        [BsonIgnore]
        public Director Director { get; set; }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}