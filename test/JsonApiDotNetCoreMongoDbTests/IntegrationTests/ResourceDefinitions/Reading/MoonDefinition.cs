using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MoonDefinition : HitCountingResourceDefinition<Moon, string?>
{
    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

    public MoonDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
        : base(resourceGraph, hitCounter)
    {
        // This constructor will be resolved from the container, which means
        // you can take on any dependency that is also defined in the container.
    }

    public override QueryStringParameterHandlers<Moon> OnRegisterQueryableHandlersForQueryStringParameters()
    {
        base.OnRegisterQueryableHandlersForQueryStringParameters();

        return new QueryStringParameterHandlers<Moon>
        {
            ["isLargerThanTheSun"] = FilterByRadius
        };
    }

    private static IQueryable<Moon> FilterByRadius(IQueryable<Moon> source, StringValues parameterValue)
    {
        // Workaround for https://youtrack.jetbrains.com/issue/RSRP-493256/Incorrect-possible-null-assignment
        // ReSharper disable once AssignNullToNotNullAttribute
        bool isFilterOnLargerThan = bool.Parse(parameterValue.ToString());
        return isFilterOnLargerThan ? source.Where(moon => moon.SolarRadius > 1m) : source.Where(moon => moon.SolarRadius <= 1m);
    }
}
