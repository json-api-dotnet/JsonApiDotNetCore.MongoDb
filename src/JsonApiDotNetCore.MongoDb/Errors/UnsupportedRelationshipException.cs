using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.MongoDb.Errors
{
    public sealed class UnsupportedRelationshipException : JsonApiException
    {
        public UnsupportedRelationshipException()
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = "Relationships are not supported when using MongoDB."
            })
        {
        }
    }
}