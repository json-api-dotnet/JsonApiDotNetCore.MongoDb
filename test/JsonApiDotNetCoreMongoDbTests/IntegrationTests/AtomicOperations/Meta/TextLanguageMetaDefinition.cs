using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Meta;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TextLanguageMetaDefinition : ContainerTypeToHideFromAutoDiscovery.ImplicitlyChangingTextLanguageDefinition
{
    internal const string NoticeText = "See https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes for ISO 639-1 language codes.";

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.GetMeta;

    public TextLanguageMetaDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter, IMongoDataAccess mongoDataAccess)
        : base(resourceGraph, hitCounter, mongoDataAccess)
    {
    }

    public override IDictionary<string, object?> GetMeta(TextLanguage resource)
    {
        base.GetMeta(resource);

        return new Dictionary<string, object?>
        {
            ["Notice"] = NoticeText
        };
    }
}
