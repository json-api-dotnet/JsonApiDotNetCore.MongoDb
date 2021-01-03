using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.MongoDb.Errors
{
    /// <summary>
    /// The error that is thrown when a filter compares two attributes.
    /// This is not supported by MongoDB.Driver.
    /// https://jira.mongodb.org/browse/CSHARP-1592
    /// </summary>
    public sealed class AttributeComparisonInFilterNotSupportedException : JsonApiException
    {
        public AttributeComparisonInFilterNotSupportedException()
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = "Comparing attributes against each other is not supported when using MongoDB."
            })
        {
        }
    }
}