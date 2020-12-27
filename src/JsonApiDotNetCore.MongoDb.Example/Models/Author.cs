using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public sealed class Author : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr]
        public DateTime? DateOfBirth { get; set; }

        [Attr]
        public string BusinessEmail { get; set; }

        // [HasOne]
        // public Address LivingAddress { get; set; }
        //
        // [HasMany]
        // public IList<Article> Articles { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
