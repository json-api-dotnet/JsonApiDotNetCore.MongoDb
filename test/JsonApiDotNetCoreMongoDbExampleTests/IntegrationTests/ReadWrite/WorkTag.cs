using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkTag : MongoIdentifiable
    {
        [Attr]
        public string Text { get; set; }

        [Attr]
        public bool IsBuiltIn { get; set; }
    }
}
