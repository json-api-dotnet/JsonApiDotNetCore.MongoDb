using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Label : MongoDbIdentifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public LabelColor Color { get; set; }

        [HasManyThrough(nameof(BlogPostLabels))]
        [BsonIgnore]
        public ISet<BlogPost> Posts { get; set; }

        [BsonIgnore]
        public ISet<BlogPostLabel> BlogPostLabels { get; set; }
    }
}
