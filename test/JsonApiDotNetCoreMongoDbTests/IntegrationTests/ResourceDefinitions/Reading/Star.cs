using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading")]
public sealed class Star : MongoIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public StarKind Kind { get; set; }

    [Attr]
    public decimal SolarRadius { get; set; }

    [Attr]
    public decimal SolarMass { get; set; }

    [Attr]
    public bool IsVisibleFromEarth { get; set; }
}
