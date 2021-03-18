using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Calendar : MongoIdentifiable
    {
        [Attr]
        public string TimeZone { get; set; }

        [Attr]
        public int DefaultAppointmentDurationInMinutes { get; set; }

        [HasMany]
        [BsonIgnore]
        public ISet<Appointment> Appointments { get; set; }
    }
}
