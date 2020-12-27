using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public sealed class Article : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr]
        public string Caption { get; set; }

        [Attr]
        public string Url { get; set; }

        // [HasOne]
        // public Author Author { get; set; }
        //
        // [BsonIgnore]
        // [HasManyThrough(nameof(ArticleTags))]
        // public ISet<Tag> Tags { get; set; }
        // public ISet<ArticleTag> ArticleTags { get; set; }
        //
        // [BsonIgnore]
        // [HasManyThrough(nameof(IdentifiableArticleTags))]
        // public ICollection<Tag> IdentifiableTags { get; set; }
        // public ICollection<IdentifiableArticleTag> IdentifiableArticleTags { get; set; }
        //
        // [HasMany]
        // public ICollection<Revision> Revisions { get; set; }
        //
        // [HasOne]
        // public Blog Blog { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
