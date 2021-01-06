using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models
{
    public sealed class Book : MongoDbIdentifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public int PublishYear { get; set; }

        [Attr]
        public string Author { get; set; }
    }
}
