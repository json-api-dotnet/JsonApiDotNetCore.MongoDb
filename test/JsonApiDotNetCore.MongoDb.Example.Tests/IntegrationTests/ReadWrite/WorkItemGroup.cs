using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    public class WorkItemGroup : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr]
        public string Name { get; set; }
        
        [Attr]
        public bool IsPublic { get; set; }

        [BsonIgnore]
        [Attr]
        public Guid ConcurrencyToken => Guid.NewGuid();

        [HasOne]
        [BsonIgnore]
        public RgbColor Color { get; set; }

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}