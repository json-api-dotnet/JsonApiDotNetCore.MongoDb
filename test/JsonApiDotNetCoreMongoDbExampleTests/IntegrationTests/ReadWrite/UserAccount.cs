using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    public sealed class UserAccount : MongoDbIdentifiable
    {
        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }
    }
}
