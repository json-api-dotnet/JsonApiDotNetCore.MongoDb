using System.Collections.Generic;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class Article : MongoDbIdentifiable
    {
        [Attr]
        public string Caption { get; set; }

        [Attr]
        public string Url { get; set; }
        
        [HasOne]
        [BsonIgnore]
        public Author Author { get; set; }
        
        [BsonIgnore]
        [HasManyThrough(nameof(ArticleTags))]
        public ISet<Tag> Tags { get; set; }
        
        [BsonIgnore]
        public ISet<ArticleTag> ArticleTags { get; set; }
    }
}
