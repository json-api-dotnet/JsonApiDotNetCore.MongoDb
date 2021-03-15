using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    public sealed class ModelWithIntIdsController : JsonApiController<ModelWithIntId>
    {
        public ModelWithIntIdsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ModelWithIntId> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
