using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    public sealed class ModelWithIntId : Identifiable
    {
        [Attr]
        public string Description { get; set; }
    }
}