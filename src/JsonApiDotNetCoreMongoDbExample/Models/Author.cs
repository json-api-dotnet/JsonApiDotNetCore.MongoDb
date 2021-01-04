using System;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public sealed class Author : MongoDbIdentifiable
    {
        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr]
        public DateTime? DateOfBirth { get; set; }

        [Attr]
        public string BusinessEmail { get; set; }
    }
}
