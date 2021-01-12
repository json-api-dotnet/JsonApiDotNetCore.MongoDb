using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCoreMongoDbExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mongo2Go;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreMongoDbExampleTests
{
    /// <summary>
    /// A test context that creates a new database and server instance before running tests and cleans up afterwards.
    /// You can either use this as a fixture on your tests class (init/cleanup runs once before/after all tests) or
    /// have your tests class inherit from it (init/cleanup runs once before/after each test). See
    /// <see href="https://xunit.net/docs/shared-context"/> for details on shared context usage.
    /// </summary>
    /// <typeparam name="TStartup">The server Startup class, which can be defined in the test project.</typeparam>
    public class IntegrationTestContext<TStartup> : IDisposable
        where TStartup : class
    {
        private readonly Lazy<WebApplicationFactory<EmptyStartup>> _lazyFactory;

        private Action<IServiceCollection> _beforeServicesConfiguration;
        private Action<IServiceCollection> _afterServicesConfiguration;
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

                services.AddJsonApi(ConfigureJsonApiOptions, facade => facade.AddCurrentAssembly());
                services.AddJsonApiMongoDb();

                services.AddScoped(typeof(IResourceReadRepository<>), typeof(MongoDbRepository<>));
                services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoDbRepository<,>));
                services.AddScoped(typeof(IResourceWriteRepository<>), typeof(MongoDbRepository<>));
                services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoDbRepository<,>));
                services.AddScoped(typeof(IResourceRepository<>), typeof(MongoDbRepository<>));
                services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoDbRepository<,>));
            });

            factory.ConfigureServicesAfterStartup(_afterServicesConfiguration);

            return factory;
        }

        private void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            options.IncludeExceptionStackTraceInErrors = true;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
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

        public async Task RunOnDatabaseAsync(Func<IMongoDatabase, Task> asyncAction)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetService<IMongoDatabase>();

            await asyncAction(db);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteGetAsync<TResponseDocument>(string requestUrl,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Get, requestUrl, null, null, acceptHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecutePostAsync<TResponseDocument>(string requestUrl, object requestBody,
                string contentType = HeaderConstants.MediaType,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody, contentType,
                acceptHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecutePatchAsync<TResponseDocument>(string requestUrl, object requestBody,
                string contentType = HeaderConstants.MediaType,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Patch, requestUrl, requestBody, contentType,
                acceptHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteDeleteAsync<TResponseDocument>(string requestUrl, object requestBody = null,
                string contentType = HeaderConstants.MediaType,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Delete, requestUrl, requestBody, contentType,
                acceptHeaders);
        }

        private async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteRequestAsync<TResponseDocument>(HttpMethod method, string requestUrl, object requestBody,
                string contentType, IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders)
        {
            var request = new HttpRequestMessage(method, requestUrl);
            string requestText = SerializeRequest(requestBody);

            if (!string.IsNullOrEmpty(requestText))
            {
                request.Content = new StringContent(requestText);

                if (contentType != null)
                {
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }

            using HttpClient client = Factory.CreateClient();

            if (acceptHeaders != null)
            {
                foreach (var acceptHeader in acceptHeaders)
                {
                    client.DefaultRequestHeaders.Accept.Add(acceptHeader);
                }
            }

            HttpResponseMessage responseMessage = await client.SendAsync(request);

            string responseText = await responseMessage.Content.ReadAsStringAsync();
            var responseDocument = DeserializeResponse<TResponseDocument>(responseText);

            return (responseMessage, responseDocument);
        }

        private string SerializeRequest(object requestBody)
        {
            return requestBody == null
                ? null
                : requestBody is string stringRequestBody
                    ? stringRequestBody
                    : JsonConvert.SerializeObject(requestBody);
        }

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
                        webBuilder.ConfigureServices(services =>
                        {
                            _beforeServicesConfiguration?.Invoke(services);
                        });
                    
                        webBuilder.UseStartup<TStartup>();

                        webBuilder.ConfigureServices(services =>
                        {
                            _afterServicesConfiguration?.Invoke(services);
                        });
                    });
            }
        }
    }
}
