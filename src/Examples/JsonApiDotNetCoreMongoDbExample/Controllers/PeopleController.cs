using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExample.Controllers
{
    public sealed class PeopleController : JsonApiController<Person, string>
    {
        public PeopleController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Person, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
