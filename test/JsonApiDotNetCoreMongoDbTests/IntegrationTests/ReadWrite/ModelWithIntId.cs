using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite")]
public sealed class ModelWithIntId : Identifiable<int>
{
    [Attr]
    public string? Description { get; set; }
}
