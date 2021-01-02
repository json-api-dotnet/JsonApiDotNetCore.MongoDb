using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    public sealed class RgbColor : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr]
        public string DisplayName { get; set; }
        
        [HasOne]
        [BsonIgnore]
        public WorkItemGroup Group { get; set; }
        public MongoDBRef GroupId => new MongoDBRef(nameof(WorkItemGroup), Group.Id);

        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
