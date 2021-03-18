using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Comment : MongoIdentifiable
    {
        [Attr]
        public string Text { get; set; }

        [Attr]
        public DateTime CreatedAt { get; set; }

        [HasOne]
        [BsonIgnore]
        public WebAccount Author { get; set; }

        [HasOne]
        [BsonIgnore]
        public BlogPost Parent { get; set; }
    }
}
