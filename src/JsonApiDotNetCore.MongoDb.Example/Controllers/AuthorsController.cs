using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
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
