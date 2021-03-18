using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WebAccount : MongoIdentifiable
    {
        [Attr]
        public string UserName { get; set; }

        [Attr(Capabilities = ~AttrCapabilities.AllowView)]
        public string Password { get; set; }

        [Attr]
        public string DisplayName { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
        public DateTime? DateOfBirth { get; set; }

        [Attr]
        public string EmailAddress { get; set; }

        [HasMany]
        [BsonIgnore]
        public IList<BlogPost> Posts { get; set; }

        [HasOne]
        [BsonIgnore]
        public AccountPreferences Preferences { get; set; }
    }
}
