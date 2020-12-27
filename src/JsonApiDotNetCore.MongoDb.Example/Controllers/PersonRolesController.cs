using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
{
    public sealed class PersonRolesController : JsonApiController<PersonRole, string>
    {
        public PersonRolesController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<PersonRole, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
