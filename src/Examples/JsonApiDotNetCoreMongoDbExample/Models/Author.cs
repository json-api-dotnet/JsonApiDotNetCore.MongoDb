using System;
using System.Collections.Generic;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class Author : MongoDbIdentifiable
    {
        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr]
        public DateTime? DateOfBirth { get; set; }

        [Attr]
        public string BusinessEmail { get; set; }
        
        [HasOne]
        [BsonIgnore]
        public Address LivingAddress { get; set; }
        
        [HasMany]
        [BsonIgnore]
        public IList<Article> Articles { get; set; }
    }
}
