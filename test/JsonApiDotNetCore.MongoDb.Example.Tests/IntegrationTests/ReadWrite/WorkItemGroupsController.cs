using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests
{
    public class WorkItemGroupsController : JsonApiController<WorkItemGroup, string>
    {
        public WorkItemGroupsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<WorkItemGroup, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}