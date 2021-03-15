using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings.SparseFieldSets
{
    public sealed class ResourceCaptureStore
    {
        internal List<IIdentifiable> Resources { get; } = new List<IIdentifiable>();

        internal void Add(IEnumerable<IIdentifiable> resources)
        {
            Resources.AddRange(resources);
        }

        internal void Clear()
        {
            Resources.Clear();
        }
    }
}
