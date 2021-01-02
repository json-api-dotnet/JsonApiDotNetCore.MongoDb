using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Tests.Helpers.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.Helpers.Controllers
{
    public class MoviesController : JsonApiController<Movie, string>
    {
        public MoviesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Movie, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}