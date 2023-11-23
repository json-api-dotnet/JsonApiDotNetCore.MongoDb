using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.SparseFieldSets;

public sealed class ResourceCaptureStore
{
    internal List<IIdentifiable> Resources { get; } = [];

    internal void Add(IEnumerable<IIdentifiable> resources)
    {
        Resources.AddRange(resources);
    }

    internal void Clear()
    {
        Resources.Clear();
    }
}
