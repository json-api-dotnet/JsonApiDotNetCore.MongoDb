using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MoonDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Moon, string?>(resourceGraph, hitCounter)
{
    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

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
