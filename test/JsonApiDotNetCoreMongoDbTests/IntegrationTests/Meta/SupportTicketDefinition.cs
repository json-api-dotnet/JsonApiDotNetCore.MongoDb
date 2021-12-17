using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class SupportTicketDefinition : HitCountingResourceDefinition<SupportTicket, string?>
{
    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.GetMeta;

    public SupportTicketDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
        : base(resourceGraph, hitCounter)
    {
    }

    public override IDictionary<string, object?>? GetMeta(SupportTicket resource)
    {
        base.GetMeta(resource);

        if (!string.IsNullOrEmpty(resource.Description) && resource.Description.StartsWith("Critical:", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?>
            {
                ["hasHighPriority"] = true
            };
        }

        return null;
    }
}
