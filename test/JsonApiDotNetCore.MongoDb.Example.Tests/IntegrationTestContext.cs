using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mongo2Go;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCore.MongoDb.Example.Tests
{
    public sealed class IntegrationTestContext<TStartup> : IDisposable
        where TStartup : class
    {
        private readonly Lazy<WebApplicationFactory<EmptyStartup>> _lazyFactory;
        
        private Action<IServiceCollection> _beforeServicesConfiguration;
        private Action<IServiceCollection> _afterServicesConfiguration;
        private Action<ResourceGraphBuilder> _registerResources;
        private readonly MongoDbRunner _runner;

        public WebApplicationFactory<EmptyStartup> Factory => _lazyFactory.Value;

        public IntegrationTestContext()
        {
            _lazyFactory = new Lazy<WebApplicationFactory<EmptyStartup>>(CreateFactory);
            _runner = MongoDbRunner.Start();
        }
        
        private WebApplicationFactory<EmptyStartup> CreateFactory()
        {
            var factory = new IntegrationTestWebApplicationFactory();
            
            factory.ConfigureServicesBeforeStartup(services =>
            {
                _beforeServicesConfiguration?.Invoke(services);
                
                services.AddSingleton(sp =>
                {
                    var client = new MongoClient(_runner.ConnectionString);
                    return client.GetDatabase($"JsonApiDotNetCore_MongoDb_{new Random().Next()}_Test");
                });

                services.AddJsonApi(
                    options =>
                    {
                        options.IncludeExceptionStackTraceInErrors = true;
                        options.SerializerSettings.Formatting = Formatting.Indented;
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    }, resources: _registerResources);
            });

            factory.ConfigureServicesAfterStartup(_afterServicesConfiguration);

            return factory;
        }

        public void Dispose()
        {
            _runner.Dispose();
            Factory.Dispose();
        }

        public void ConfigureServicesBeforeStartup(Action<IServiceCollection> servicesConfiguration) =>
            _beforeServicesConfiguration = servicesConfiguration;

        public void ConfigureServicesAfterStartup(Action<IServiceCollection> servicesConfiguration) =>
            _afterServicesConfiguration = servicesConfiguration;

        public void RegisterResources(Action<ResourceGraphBuilder> resources) =>
            _registerResources = resources;

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
        
        private sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<EmptyStartup>
        {
            private Action<IServiceCollection> _beforeServicesConfiguration;
            private Action<IServiceCollection> _afterServicesConfiguration;

            public void ConfigureServicesBeforeStartup(Action<IServiceCollection> servicesConfiguration)
            {
                _beforeServicesConfiguration = servicesConfiguration;
            }

            public void ConfigureServicesAfterStartup(Action<IServiceCollection> servicesConfiguration)
            {
                _afterServicesConfiguration = servicesConfiguration;
            }

            protected override IHostBuilder CreateHostBuilder()
            {
                return Host.CreateDefaultBuilder(null)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureTestServices(services =>
                        {
                            _beforeServicesConfiguration?.Invoke(services);
                        });
                    
                        webBuilder.UseStartup<TStartup>();

                        webBuilder.ConfigureTestServices(services =>
                        {
                            _afterServicesConfiguration?.Invoke(services);
                        });
                    });
            }
        }
    }
}
