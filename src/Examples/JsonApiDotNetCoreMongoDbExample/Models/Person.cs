using System.Linq;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class Person : MongoDbIdentifiable, IIsLockable
    {
        private string _firstName;

        public bool IsLocked { get; set; }

        [Attr]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (value != _firstName)
                {
                    _firstName = value;
                    Initials = string.Concat(value.Split(' ').Select(x => char.ToUpperInvariant(x[0])));
                }
            }
        }

        [Attr]
        public string Initials { get; set; }

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
