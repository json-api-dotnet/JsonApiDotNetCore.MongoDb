using Example.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace Example.Controllers
{
    public sealed class BooksController : JsonApiController<Book, string>
    {
        public BooksController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Book, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
