using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class AtomicOperationsFixture : IAsyncLifetime
{
    internal IntegrationTestContext<TestableStartup, OperationsDbContext> TestContext { get; }

    public AtomicOperationsFixture()
    {
        TestContext = new IntegrationTestContext<TestableStartup, OperationsDbContext>
        {
            StartMongoDbInSingleNodeReplicaSetMode = true
        };

        TestContext.UseController<OperationsController>();

        TestContext.ConfigureServicesAfterStartup(services => services.AddSingleton<ResourceDefinitionHitCounter>());
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
