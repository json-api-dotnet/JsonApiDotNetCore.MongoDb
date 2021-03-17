using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Meta
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class MusicTrackMetaDefinition : JsonApiResourceDefinition<MusicTrack, string>
    {
        public MusicTrackMetaDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }

        public override IDictionary<string, object> GetMeta(MusicTrack resource)
        {
            return new Dictionary<string, object>
            {
                ["Copyright"] = $"(C) {resource.ReleasedAt.Year}. All rights reserved."
            };
        }
    }
}
