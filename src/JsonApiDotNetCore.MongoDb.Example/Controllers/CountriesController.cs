using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.MongoDb.Example.Controllers
{
    [DisableQueryString(StandardQueryStringParameters.Sort | StandardQueryStringParameters.Page)]
    public sealed class CountriesController : JsonApiController<Country, string>
    {
        public CountriesController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Country, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
