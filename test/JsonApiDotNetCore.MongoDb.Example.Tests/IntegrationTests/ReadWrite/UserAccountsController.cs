using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    public sealed class UserAccountsController : JsonApiController<UserAccount, string>
    {
        public UserAccountsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<UserAccount, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
