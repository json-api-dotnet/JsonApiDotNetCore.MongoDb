using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExample.Controllers
{
    public sealed class BlogsController : JsonApiController<Blog, string>
    {
        public BlogsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Blog, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
