using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
{
    public sealed class PassportsController : JsonApiController<Passport, string>
    {
        public PassportsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Passport, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
