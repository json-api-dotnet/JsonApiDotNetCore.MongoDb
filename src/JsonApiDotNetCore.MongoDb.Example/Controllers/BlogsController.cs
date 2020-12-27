using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
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
