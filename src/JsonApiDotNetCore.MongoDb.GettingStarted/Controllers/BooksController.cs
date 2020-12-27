using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.GettingStarted.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.GettingStarted.Controllers
{
    public sealed class BooksController : JsonApiController<Book, string>
    {
        public BooksController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Book, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
