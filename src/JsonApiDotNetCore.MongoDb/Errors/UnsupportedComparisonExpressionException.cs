using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.MongoDb.Errors
{
    public sealed class UnsupportedComparisonExpressionException : JsonApiException
    {
        public UnsupportedComparisonExpressionException()
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = "Comparing attributes against each other is not supported when using MongoDB."
            })
        {
        }
    }
}