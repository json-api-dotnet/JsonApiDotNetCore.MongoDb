using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    public sealed class BlogsController : JsonApiController<Blog, string>
    {
        public BlogsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Blog, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
