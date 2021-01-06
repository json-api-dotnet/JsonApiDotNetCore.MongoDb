using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExample.Controllers
{
    public sealed class TodoItemsController : JsonApiController<TodoItem, string>
    {
        public TodoItemsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<TodoItem, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
