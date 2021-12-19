using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StarKind
{
    Other,
    RedDwarf,
    MainSequence,
    RedGiant
}
