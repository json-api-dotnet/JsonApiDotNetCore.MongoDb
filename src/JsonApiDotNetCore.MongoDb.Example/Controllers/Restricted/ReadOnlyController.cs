using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers.Restricted
{
    [DisableRoutingConvention, Route("[controller]")]
    [HttpReadOnly]
    public class ReadOnlyController : BaseJsonApiController<Article, string>
    {
        public ReadOnlyController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article, string> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [DisableRoutingConvention, Route("[controller]")]
    [NoHttpPost]
    public class NoHttpPostController : BaseJsonApiController<Article, string>
    {
        public NoHttpPostController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article, string> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [DisableRoutingConvention, Route("[controller]")]
    [NoHttpPatch]
    public class NoHttpPatchController : BaseJsonApiController<Article, string>
    {
        public NoHttpPatchController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article, string> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [DisableRoutingConvention, Route("[controller]")]
    [NoHttpDelete]
    public class NoHttpDeleteController : BaseJsonApiController<Article, string>
    {
        public NoHttpDeleteController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article, string> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }
}
