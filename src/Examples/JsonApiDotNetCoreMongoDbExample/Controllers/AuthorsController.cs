using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExample.Controllers
{
    public sealed class AuthorsController : JsonApiController<Author, string>
    {
        public AuthorsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Author, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
