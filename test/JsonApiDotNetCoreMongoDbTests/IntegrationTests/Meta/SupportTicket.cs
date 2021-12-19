using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.Meta")]
public sealed class SupportTicket : MongoIdentifiable
{
    [Attr]
    public string Description { get; set; } = null!;
}
