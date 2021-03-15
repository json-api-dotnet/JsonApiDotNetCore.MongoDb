using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItemGroup : MongoDbIdentifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsPublic { get; set; }

        [Attr]
        [BsonIgnore]
        public Guid ConcurrencyToken => Guid.NewGuid();

        [HasOne]
        [BsonIgnore]
        public RgbColor Color { get; set; }

        [HasMany]
        [BsonIgnore]
        public IList<WorkItem> Items { get; set; }
    }
}
