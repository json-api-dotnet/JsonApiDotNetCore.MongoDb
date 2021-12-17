using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LabelColor
{
    Red,
    Green,
    Blue
}
