using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
{
    [DisableQueryString("skipCache")]
    public sealed class TagsController : JsonApiController<Tag, string>
    {
        public TagsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Tag, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
