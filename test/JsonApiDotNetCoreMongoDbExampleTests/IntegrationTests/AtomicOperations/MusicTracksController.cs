using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class MusicTracksController : JsonApiController<MusicTrack, string>
    {
        public MusicTracksController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<MusicTrack, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
