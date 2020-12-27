using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
{
    public sealed class ThrowingResourcesController : JsonApiController<ThrowingResource, string>
    {
        public ThrowingResourcesController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<ThrowingResource, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
