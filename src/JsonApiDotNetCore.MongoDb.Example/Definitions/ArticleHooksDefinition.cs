using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.MongoDb.Example.Definitions
{
    public class ArticleHooksDefinition : ResourceHooksDefinition<Article>
    {
        public ArticleHooksDefinition(IResourceGraph resourceGraph) : base(resourceGraph) { }

        public override IEnumerable<Article> OnReturn(HashSet<Article> resources, ResourcePipeline pipeline)
        {
            if (pipeline == ResourcePipeline.GetSingle && resources.Any(r => r.Caption == "Classified"))
            {
                throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                {
                    Title = "You are not allowed to see this article."
                });
            }

            return resources.Where(t => t.Caption != "This should not be included");
        }
    }
}

