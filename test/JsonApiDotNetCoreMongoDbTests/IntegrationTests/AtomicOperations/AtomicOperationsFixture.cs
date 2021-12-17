using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class AtomicOperationsFixture : IDisposable
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

    public void Dispose()
    {
        TestContext.Dispose();
    }
}
