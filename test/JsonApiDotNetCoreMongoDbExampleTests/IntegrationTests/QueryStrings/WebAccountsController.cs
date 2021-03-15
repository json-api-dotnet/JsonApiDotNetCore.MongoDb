using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    public sealed class WebAccountsController : JsonApiController<WebAccount, string>
    {
        public WebAccountsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<WebAccount, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
