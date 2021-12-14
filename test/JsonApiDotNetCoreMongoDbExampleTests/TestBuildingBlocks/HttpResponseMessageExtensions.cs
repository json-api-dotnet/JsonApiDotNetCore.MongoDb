using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Primitives;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks
{
    [PublicAPI]
    public static class HttpResponseMessageExtensions
    {
        public static HttpResponseMessageAssertions Should(this HttpResponseMessage instance)
        {
            return new HttpResponseMessageAssertions(instance);
        }

        public sealed class HttpResponseMessageAssertions : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>
        {
            protected override string Identifier => "response";

            public HttpResponseMessageAssertions(HttpResponseMessage subject)
                : base(subject)
            {
            }

            // ReSharper disable once UnusedMethodReturnValue.Global
            [CustomAssertion]
            internal AndConstraint<HttpResponseMessageAssertions> HaveStatusCode(HttpStatusCode statusCode)
            {
                if (Subject.StatusCode != statusCode)
                {
                    string responseText = GetFormattedContentAsync(Subject).Result;
                    Subject.StatusCode.Should().Be(statusCode, "response body returned was:\n" + responseText);
                }

                return new AndConstraint<HttpResponseMessageAssertions>(this);
            }

            private static async Task<string> GetFormattedContentAsync(HttpResponseMessage responseMessage)
            {
                string text = await responseMessage.Content.ReadAsStringAsync();

                try
                {
                    if (text.Length > 0)
                    {
                        var json = JsonConvert.DeserializeObject<JObject>(text);

                        if (json != null)
                        {
                            return json.ToString();
                        }
                    }
                }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
                catch
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
                {
                    // ignored
                }

                return text;
            }
        }
    }
}
