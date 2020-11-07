using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Example;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.MongoDb.Example.Tests.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.MongoDb.Example.Tests
{
    public sealed class IntegrationTestContext<TStartup> : IDisposable
        where TStartup : class
    {
        private readonly Lazy<WebApplicationFactory<Startup>> _lazyFactory;
        
        private Action<IServiceCollection> _beforeServicesConfiguration;
        private Action<IServiceCollection> _afterServicesConfiguration;

        private WebApplicationFactory<Startup> Factory => _lazyFactory.Value;

        public IntegrationTestContext()
        {
            _lazyFactory = new Lazy<WebApplicationFactory<Startup>>(CreateFactory);
        }
        
        private WebApplicationFactory<Startup> CreateFactory()
        {
            var factory = new IntegrationTestWebApplicationFactory<Startup>();
            
            factory.ConfigureServicesBeforeStartup(_beforeServicesConfiguration);

            factory.ConfigureServicesAfterStartup(services =>
            {
                _afterServicesConfiguration?.Invoke(services);
            });

            return factory;
        }

        public void Dispose() => Factory.Dispose();

        public void ConfigureServicesBeforeStartup(Action<IServiceCollection> servicesConfiguration) =>
            _beforeServicesConfiguration = servicesConfiguration;

        public void ConfigureServicesAfterStartup(Action<IServiceCollection> servicesConfiguration) =>
            _afterServicesConfiguration = servicesConfiguration;

        public async Task RunOnDatabaseAsync(Func<IMongoDatabase, Task> asyncAction)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetService<IMongoDatabase>();

            await asyncAction(db);
        }

        public Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteGetAsync<TResponseDocument>(string requestUrl) =>
            ExecuteRequestAsync<TResponseDocument>(HttpMethod.Get, requestUrl);

        public Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePostAsync<TResponseDocument>(string requestUrl, object requestBody) =>
            ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody);

        public Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePatchAsync<TResponseDocument>(string requestUrl, object requestBody) =>
            ExecuteRequestAsync<TResponseDocument>(HttpMethod.Patch, requestUrl, requestBody);

        public Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteDeleteAsync<TResponseDocument>(string requestUrl) =>
            ExecuteRequestAsync<TResponseDocument>(HttpMethod.Delete, requestUrl);

        private async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteRequestAsync<TResponseDocument>(HttpMethod method, string requestUrl, object requestBody = null)
        {
            var request = new HttpRequestMessage(method, requestUrl);
            var requestText = SerializeRequest(requestBody);

            if (!string.IsNullOrEmpty(requestText))
            {
                request.Content = new StringContent(requestText);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);
            }

            using var client = Factory.CreateClient();
            var responseMessage = await client.SendAsync(request);

            var responseText = await responseMessage.Content.ReadAsStringAsync();
            var responseDocument = DeserializeResponse<TResponseDocument>(responseText);

            return (responseMessage, responseDocument);
        }

        private string SerializeRequest(object requestBody) => 
            requestBody is string stringRequestBody
                ? stringRequestBody
                : JsonConvert.SerializeObject(requestBody);

        private TResponseDocument DeserializeResponse<TResponseDocument>(string responseText)
        {
            if (typeof(TResponseDocument) == typeof(string))
            {
                return (TResponseDocument)(object)responseText;
            }

            try
            {
                return JsonConvert.DeserializeObject<TResponseDocument>(responseText);
            }
            catch (JsonException exception)
            {
                throw new FormatException($"Failed to deserialize response body to JSON:\n{responseText}", exception);
            }
        }
    }
}
