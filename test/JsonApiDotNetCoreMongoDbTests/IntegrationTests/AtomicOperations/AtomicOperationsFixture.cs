using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Meta;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;
using Xunit;

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

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await TestContext.DisposeAsync();
    }
}
