using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExample.Controllers
{
    public sealed class ArticlesController : JsonApiController<Article, string>
    {
        public ArticlesController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
