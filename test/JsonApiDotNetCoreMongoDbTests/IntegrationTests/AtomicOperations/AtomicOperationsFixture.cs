using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Meta;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class AtomicOperationsFixture : IAsyncLifetime
{
    internal IntegrationTestContext<TestableStartup, OperationsDbContext> TestContext { get; } = new();

    public AtomicOperationsFixture()
    {
        TestContext.UseResourceTypesInNamespace(typeof(MusicTrack).Namespace);

        TestContext.UseController<OperationsController>();

        TestContext.ConfigureServices(services =>
        {
            services.TryAddSingleton<ResourceDefinitionHitCounter>();

            services.AddResourceDefinition<MusicTrackMetaDefinition>();
            services.AddResourceDefinition<TextLanguageMetaDefinition>();
        });
    }

    public async ValueTask InitializeAsync()
    {
        await TestContext.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await TestContext.DisposeAsync();
    }
}
