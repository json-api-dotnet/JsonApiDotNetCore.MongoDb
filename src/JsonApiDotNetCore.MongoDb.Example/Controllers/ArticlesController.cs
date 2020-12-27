using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
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
