using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.MongoDb.Example.Definitions
{
    public class TagHooksDefinition : ResourceHooksDefinition<Tag>
    {
        public TagHooksDefinition(IResourceGraph resourceGraph) : base(resourceGraph) { }

        public override IEnumerable<Tag> BeforeCreate(IResourceHashSet<Tag> affected, ResourcePipeline pipeline)
        {
            return base.BeforeCreate(affected, pipeline);
        }

        public override IEnumerable<Tag> OnReturn(HashSet<Tag> resources, ResourcePipeline pipeline)
        {
            return resources.Where(t => t.Name != "This should not be included");
        }
    }
}
