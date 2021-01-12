using System;
using System.Collections.Generic;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ResourceDefinitions
{
    public class CallableResource : MongoDbIdentifiable
    {
        [Attr]
        public string Label { get; set; }

        [Attr]
        public int PercentageComplete { get; set; }

        [Attr]
        public string Status => $"{PercentageComplete}% completed.";

        [Attr]
        public int RiskLevel { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowSort)]
        public DateTime CreatedAt { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowSort)]
        public DateTime ModifiedAt { get; set; }

        [Attr(Capabilities = AttrCapabilities.None)]
        public bool IsDeleted { get; set; }

        [HasMany]
        [BsonIgnore]
        public ICollection<CallableResource> Children { get; set; }

        [HasOne]
        [BsonIgnore]
        public CallableResource Owner { get; set; }
    }
}