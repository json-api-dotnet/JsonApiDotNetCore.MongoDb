using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public sealed class Blog : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr] 
        public string Title { get; set; }

        [Attr]
        public string CompanyName { get; set; }

        // [HasMany]
        // public IList<Article> Articles { get; set; }

        // [HasOne]
        // public Author Owner { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
