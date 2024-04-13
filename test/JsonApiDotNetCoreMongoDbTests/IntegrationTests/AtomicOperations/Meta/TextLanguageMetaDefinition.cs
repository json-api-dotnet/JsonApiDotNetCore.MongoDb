using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Meta;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TextLanguageMetaDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter, IMongoDataAccess mongoDataAccess)
    : ImplicitlyChangingTextLanguageDefinition(resourceGraph, hitCounter, mongoDataAccess)
{
    internal const string NoticeText = "See https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes for ISO 639-1 language codes.";

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.GetMeta;

    public override IDictionary<string, object?> GetMeta(TextLanguage resource)
    {
        base.GetMeta(resource);

        return new Dictionary<string, object?>
        {
            ["Notice"] = NoticeText
        };
    }
}
