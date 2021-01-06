using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public class User : MongoDbIdentifiable
    {
        [Attr] public string UserName { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public string Password { get; set;  }
    }
}
