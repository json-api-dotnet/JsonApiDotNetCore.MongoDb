using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
{
    public sealed class TodoCollectionsController : JsonApiController<TodoItemCollection, string>
    {

        public TodoCollectionsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<TodoItemCollection, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(string id, [FromBody] TodoItemCollection resource, CancellationToken cancellationToken)
        {
            // if (resource.Name == "PRE-ATTACH-TEST")
            // {
            //     var targetTodoId = resource.TodoItems.First().Id;
            //     var todoItemContext = _dbResolver.GetContext().Set<TodoItem>();
            //     await todoItemContext.Where(ti => ti.Id == targetTodoId).FirstOrDefaultAsync(cancellationToken);
            // }

            return await base.PatchAsync(id, resource, cancellationToken);
        }

    }
}
