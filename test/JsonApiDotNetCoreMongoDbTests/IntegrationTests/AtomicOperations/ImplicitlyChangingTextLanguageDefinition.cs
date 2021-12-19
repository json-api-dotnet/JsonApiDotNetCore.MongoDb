using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.MongoDb.Repositories;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly]
public sealed class ContainerTypeToHideFromAutoDiscovery
{
    /// <summary>
    /// Used to simulate side effects that occur in the database while saving, typically caused by database triggers.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public abstract class ImplicitlyChangingTextLanguageDefinition : HitCountingResourceDefinition<TextLanguage, string?>
    {
        internal const string Suffix = " (changed)";

        private readonly IMongoDataAccess _mongoDataAccess;

        protected ImplicitlyChangingTextLanguageDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter,
            IMongoDataAccess mongoDataAccess)
            : base(resourceGraph, hitCounter)
        {
            _mongoDataAccess = mongoDataAccess;
        }

        public override async Task OnWriteSucceededAsync(TextLanguage resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnWriteSucceededAsync(resource, writeOperation, cancellationToken);

            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                resource.IsoCode += Suffix;

                FilterDefinition<TextLanguage> filter = Builders<TextLanguage>.Filter.Eq(item => item.Id, resource.Id);

                IMongoCollection<TextLanguage> collection = _mongoDataAccess.MongoDatabase.GetCollection<TextLanguage>(nameof(TextLanguage));
                await collection.ReplaceOneAsync(_mongoDataAccess.ActiveSession, filter, resource, cancellationToken: cancellationToken);
            }
        }
    }
}
