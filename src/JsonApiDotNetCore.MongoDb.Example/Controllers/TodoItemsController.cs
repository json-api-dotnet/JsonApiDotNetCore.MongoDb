using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
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
