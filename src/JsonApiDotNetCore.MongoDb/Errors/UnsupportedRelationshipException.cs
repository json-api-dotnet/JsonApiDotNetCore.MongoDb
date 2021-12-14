using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.MongoDb.Errors;

/// <summary>
/// The error that is thrown when the user attempts to fetch, create or update a relationship.
/// </summary>
[PublicAPI]
public sealed class UnsupportedRelationshipException : JsonApiException
{
    public UnsupportedRelationshipException()
        : base(new ErrorObject(HttpStatusCode.BadRequest)
        {
            Title = "Relationships are not supported when using MongoDB."
        })
    {
    }
}
