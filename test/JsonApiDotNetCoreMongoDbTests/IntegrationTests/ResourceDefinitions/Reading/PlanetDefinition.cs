using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PlanetDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Planet, string?>(resourceGraph, hitCounter)
{
    private readonly IClientSettingsProvider _clientSettingsProvider = clientSettingsProvider;

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        base.OnApplyFilter(existingFilter);

        if (_clientSettingsProvider.ArePlanetsWithPrivateNameHidden)
        {
            AttrAttribute privateNameAttribute = ResourceType.GetAttributeByPropertyName(nameof(Planet.PrivateName));

            FilterExpression hasNoPrivateName = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(privateNameAttribute),
                NullConstantExpression.Instance);

            return LogicalExpression.Compose(LogicalOperator.And, hasNoPrivateName, existingFilter);
        }

        return existingFilter;
    }
}
