using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkItemPriority
{
    Low,
    Medium,
    High
}
