using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class BlogPost : MongoIdentifiable
    {
        [Attr]
        public string Caption { get; set; }

        [Attr]
        public string Url { get; set; }

        [HasOne]
        [BsonIgnore]
        public WebAccount Author { get; set; }

        [HasOne]
        [BsonIgnore]
        public WebAccount Reviewer { get; set; }

        [HasManyThrough(nameof(BlogPostLabels))]
        [BsonIgnore]
        public ISet<Label> Labels { get; set; }

        [BsonIgnore]
        public ISet<BlogPostLabel> BlogPostLabels { get; set; }

        [HasMany]
        [BsonIgnore]
        public ISet<Comment> Comments { get; set; }

        [HasOne(CanInclude = false)]
        [BsonIgnore]
        public Blog Parent { get; set; }
    }
}
