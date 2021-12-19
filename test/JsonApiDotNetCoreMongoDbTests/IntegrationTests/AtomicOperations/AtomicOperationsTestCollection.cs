using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[CollectionDefinition("AtomicOperationsFixture")]
public sealed class AtomicOperationsTestCollection : ICollectionFixture<AtomicOperationsFixture>
{
    // Starting MongoDB in Single Node Replica Set mode is required to enable transactions.
    // Starting in this mode requires about 10 seconds, which is normally repeated for each test class.
    // So to improve test runtime performance, we reuse a single MongoDB instance for all atomic:operations tests.
}
