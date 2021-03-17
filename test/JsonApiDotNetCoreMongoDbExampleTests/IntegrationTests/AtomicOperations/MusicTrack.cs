using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class MusicTrack : MongoDbIdentifiable
    {
        [RegularExpression(@"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$")]
        public override string Id { get; set; }

        [Attr]
        [Required]
        public string Title { get; set; }

        [Attr]
        [Range(1, 24 * 60)]
        public decimal? LengthInSeconds { get; set; }

        [Attr]
        public string Genre { get; set; }

        [Attr]
        public DateTimeOffset ReleasedAt { get; set; }

        [HasOne]
        [BsonIgnore]
        public Lyric Lyric { get; set; }

        [HasOne]
        [BsonIgnore]
        public RecordCompany OwnedBy { get; set; }

        [HasMany]
        [BsonIgnore]
        public IList<Performer> Performers { get; set; }
    }
}
