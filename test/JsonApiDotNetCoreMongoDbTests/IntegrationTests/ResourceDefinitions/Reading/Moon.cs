using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading")]
public sealed class Moon : HexStringMongoIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public decimal SolarRadius { get; set; }
}
