using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Filtering
{
    public sealed class FilterableResourcesController : JsonApiController<FilterableResource, string>
    {
        public FilterableResourcesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<FilterableResource, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
