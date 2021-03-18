using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SupportTicket : MongoIdentifiable
    {
        [Attr]
        public string Description { get; set; }
    }
}
