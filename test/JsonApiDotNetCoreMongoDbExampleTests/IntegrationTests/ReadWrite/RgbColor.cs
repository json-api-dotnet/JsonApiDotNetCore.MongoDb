using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class RgbColor : MongoDbIdentifiable
    {
        [Attr]
        public string DisplayName { get; set; }

        [HasOne]
        [BsonIgnore]
        public WorkItemGroup Group { get; set; }
    }
}
