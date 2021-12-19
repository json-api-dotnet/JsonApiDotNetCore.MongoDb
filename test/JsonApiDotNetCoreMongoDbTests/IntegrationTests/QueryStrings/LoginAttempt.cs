using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LoginAttempt : MongoIdentifiable
{
    [Attr]
    public DateTimeOffset TriedAt { get; set; }

    [Attr]
    public bool IsSucceeded { get; set; }
}
