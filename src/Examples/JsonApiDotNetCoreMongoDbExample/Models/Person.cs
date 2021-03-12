using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class Person : MongoDbIdentifiable, IIsLockable
    {
        public bool IsLocked { get; set; }

        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr(PublicName = "the-Age")]
        public int Age { get; set; }

        [Attr]
        public Gender Gender { get; set; }

        [Attr]
        public string Category { get; set; }
    }
}
