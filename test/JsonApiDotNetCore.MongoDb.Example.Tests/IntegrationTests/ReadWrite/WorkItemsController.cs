using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    public sealed class WorkItemsController : JsonApiController<WorkItem, string>
    {
        public WorkItemsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<WorkItem, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
