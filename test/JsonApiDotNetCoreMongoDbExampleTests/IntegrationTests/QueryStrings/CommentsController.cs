using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    public sealed class CommentsController : JsonApiController<Comment, string>
    {
        public CommentsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Comment, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
