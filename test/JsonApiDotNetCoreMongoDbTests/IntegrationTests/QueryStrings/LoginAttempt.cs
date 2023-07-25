using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings")]
public sealed class LoginAttempt : HexStringMongoIdentifiable
{
    [Attr]
    public DateTimeOffset TriedAt { get; set; }

    [Attr]
    public bool IsSucceeded { get; set; }
}
