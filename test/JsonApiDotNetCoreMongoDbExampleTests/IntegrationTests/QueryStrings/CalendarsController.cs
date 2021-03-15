using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    public sealed class CalendarsController : JsonApiController<Calendar, string>
    {
        public CalendarsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Calendar, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
