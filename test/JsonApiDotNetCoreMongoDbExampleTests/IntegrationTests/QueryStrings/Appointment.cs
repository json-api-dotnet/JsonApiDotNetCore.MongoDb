using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Appointment : MongoIdentifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public DateTimeOffset StartTime { get; set; }

        [Attr]
        public DateTimeOffset EndTime { get; set; }
    }
}
