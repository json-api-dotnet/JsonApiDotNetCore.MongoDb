using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExample.Controllers
{
    public sealed class UsersController : JsonApiController<User, string>
    {
        public UsersController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<User, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
