using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    public sealed class WorkItemGroupsController : JsonApiController<WorkItemGroup, string>
    {
        public WorkItemGroupsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<WorkItemGroup, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
