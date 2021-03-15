using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    public sealed class BlogPostsController : JsonApiController<BlogPost, string>
    {
        public BlogPostsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<BlogPost, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
