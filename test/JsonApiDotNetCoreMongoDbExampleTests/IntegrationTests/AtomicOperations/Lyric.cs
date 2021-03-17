using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Lyric : MongoDbIdentifiable
    {
        [Attr]
        public string Format { get; set; }

        [Attr]
        public string Text { get; set; }

        [Attr(Capabilities = AttrCapabilities.None)]
        public DateTimeOffset CreatedAt { get; set; }

        [HasOne]
        [BsonIgnore]
        public TextLanguage Language { get; set; }

        [HasOne]
        [BsonIgnore]
        public MusicTrack Track { get; set; }
    }
}
