using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class Address : MongoDbIdentifiable
    {
        [Attr]
        public string Street { get; set; }

        [Attr]
        public string ZipCode { get; set; }

        [HasOne]
        [BsonIgnore]
        public Country Country { get; set; }
    }
}