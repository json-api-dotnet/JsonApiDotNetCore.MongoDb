using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TextLanguage : MongoIdentifiable
    {
        [Attr]
        public string IsoCode { get; set; }

        [Attr(Capabilities = AttrCapabilities.None)]
        [BsonIgnore]
        public Guid ConcurrencyToken
        {
            get => Guid.NewGuid();
            set => _ = value;
        }

        [HasMany]
        [BsonIgnore]
        public ICollection<Lyric> Lyrics { get; set; }
    }
}
