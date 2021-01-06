using System.Collections.Generic;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class Blog : MongoDbIdentifiable
    {
        [Attr] 
        public string Title { get; set; }

        [Attr]
        public string CompanyName { get; set; }
        
        [HasMany]
        [BsonIgnore]
        public IList<Article> Articles { get; set; }

        [HasOne]
        [BsonIgnore]
        public Author Owner { get; set; }
    }
}
