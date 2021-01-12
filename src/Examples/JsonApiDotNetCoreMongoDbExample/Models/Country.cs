using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public class Country : MongoDbIdentifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}