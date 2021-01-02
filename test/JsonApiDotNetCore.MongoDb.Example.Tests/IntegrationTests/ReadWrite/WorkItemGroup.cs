using System;
using JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests
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

        public MongoDBRef ColorId => new MongoDBRef(nameof(RgbColor), Color.Id);

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}