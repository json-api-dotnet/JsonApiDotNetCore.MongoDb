using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
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

    public sealed class SuperUsersController : JsonApiController<SuperUser, string>
    {
        public SuperUsersController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<SuperUser, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
