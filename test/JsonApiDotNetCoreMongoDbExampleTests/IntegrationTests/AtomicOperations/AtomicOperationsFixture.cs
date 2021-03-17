using System;
using JetBrains.Annotations;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class AtomicOperationsFixture : IDisposable
    {
        internal IntegrationTestContext<TestableStartup> TestContext { get; }

        public AtomicOperationsFixture()
        {
            TestContext = new IntegrationTestContext<TestableStartup>
            {
                StartMongoDbInSingleNodeReplicaSetMode = true
            };
        }

        public void Dispose()
        {
            TestContext.Dispose();
        }
    }
}
