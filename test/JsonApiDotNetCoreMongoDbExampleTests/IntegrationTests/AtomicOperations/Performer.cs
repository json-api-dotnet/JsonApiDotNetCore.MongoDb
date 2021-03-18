using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Performer : MongoIdentifiable
    {
        [Attr]
        public string ArtistName { get; set; }

        [Attr]
        public DateTimeOffset BornAt { get; set; }
    }
}
