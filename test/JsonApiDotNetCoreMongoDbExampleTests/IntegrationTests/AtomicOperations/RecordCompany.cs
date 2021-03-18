using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class RecordCompany : MongoDbIdentifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string CountryOfResidence { get; set; }

        [HasMany]
        [BsonIgnore]
        public IList<MusicTrack> Tracks { get; set; }

        [HasOne]
        [BsonIgnore]
        public RecordCompany Parent { get; set; }
    }
}
