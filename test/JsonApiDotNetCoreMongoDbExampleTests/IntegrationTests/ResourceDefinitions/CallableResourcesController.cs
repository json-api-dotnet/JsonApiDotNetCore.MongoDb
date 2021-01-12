using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ResourceDefinitions
{
    public class CallableResourcesController : JsonApiController<CallableResource, string>
    {
        public CallableResourcesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<CallableResource, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}