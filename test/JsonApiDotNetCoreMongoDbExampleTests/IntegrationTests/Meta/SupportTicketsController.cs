using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Meta
{
    public sealed class SupportTicketsController : JsonApiController<SupportTicket, string>
    {
        public SupportTicketsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SupportTicket, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
