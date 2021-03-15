using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserAccount : MongoDbIdentifiable
    {
        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [HasMany]
        [BsonIgnore]
        public ISet<WorkItem> AssignedItems { get; set; }
    }
}
