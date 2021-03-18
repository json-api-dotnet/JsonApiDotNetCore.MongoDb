using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Blog : MongoIdentifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public string PlatformName { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public bool ShowAdvertisements => PlatformName.EndsWith("(using free account)", StringComparison.Ordinal);

        [HasMany]
        [BsonIgnore]
        public IList<BlogPost> Posts { get; set; }

        [HasOne]
        [BsonIgnore]
        public WebAccount Owner { get; set; }
    }
}
