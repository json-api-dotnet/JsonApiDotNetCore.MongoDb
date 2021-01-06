using System.Collections.Generic;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public class Tag : MongoDbIdentifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public TagColor Color { get; set; }

        [HasManyThrough(nameof(ArticleTags))]
        [BsonIgnore]
        public ISet<Article> Articles { get; set; }
        public ISet<ArticleTag> ArticleTags { get; set; }
    }
}