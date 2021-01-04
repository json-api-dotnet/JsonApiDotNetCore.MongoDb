using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.SparseFieldSets
{
    public sealed class ResourceCaptureStore
    {
        public List<IIdentifiable> Resources { get; } = new List<IIdentifiable>();

        public void Add(IEnumerable<IIdentifiable> resources)
        {
            Resources.AddRange(resources);
        }

        public void Clear()
        {
            Resources.Clear();
        }
    }
}
