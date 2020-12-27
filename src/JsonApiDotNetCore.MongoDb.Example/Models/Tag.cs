using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public class Tag : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr]
        public string Name { get; set; }

        [Attr]
        public TagColor Color { get; set; }

        // [NotMapped]
        // [HasManyThrough(nameof(ArticleTags))]
        // public ISet<Article> Articles { get; set; }
        // public ISet<ArticleTag> ArticleTags { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
