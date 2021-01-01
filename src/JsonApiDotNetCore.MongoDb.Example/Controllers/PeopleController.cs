using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
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
