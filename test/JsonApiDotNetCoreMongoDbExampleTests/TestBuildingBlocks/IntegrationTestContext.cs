using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCoreMongoDbExample.Startups;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mongo2Go;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks
{
    /// <summary>
    /// A test context that creates a new database and server instance before running tests and cleans up afterwards. You can either use this as a fixture on
    /// your tests class (init/cleanup runs once before/after all tests) or have your tests class inherit from it (init/cleanup runs once before/after each
    /// test). See <see href="https://xunit.net/docs/shared-context" /> for details on shared context usage.
    /// </summary>
    /// <typeparam name="TStartup">
    /// The server Startup class, which can be defined in the test project.
    /// </typeparam>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class IntegrationTestContext<TStartup> : IntegrationTest, IDisposable
        where TStartup : class
    {
        private readonly Lazy<WebApplicationFactory<EmptyStartup>> _lazyFactory;
        private readonly MongoDbRunner _runner;

        private Action<IServiceCollection> _beforeServicesConfiguration;
        private Action<IServiceCollection> _afterServicesConfiguration;

        internal WebApplicationFactory<EmptyStartup> Factory => _lazyFactory.Value;

        public IntegrationTestContext()
        {
            _lazyFactory = new Lazy<WebApplicationFactory<EmptyStartup>>(CreateFactory);
            _runner = MongoDbRunner.Start();
        }

        protected override HttpClient CreateClient()
        {
            return Factory.CreateClient();
        }

        private WebApplicationFactory<EmptyStartup> CreateFactory()
        {
            var factory = new IntegrationTestWebApplicationFactory();

            factory.ConfigureServicesBeforeStartup(services =>
            {
                _beforeServicesConfiguration?.Invoke(services);

                services.AddSingleton(_ =>
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

        internal void ConfigureServicesBeforeStartup(Action<IServiceCollection> servicesConfiguration)
        {
            _beforeServicesConfiguration = servicesConfiguration;
        }

        internal void ConfigureServicesAfterStartup(Action<IServiceCollection> servicesConfiguration)
        {
            _afterServicesConfiguration = servicesConfiguration;
        }

        internal async Task RunOnDatabaseAsync(Func<IMongoDatabase, Task> asyncAction)
        {
            using IServiceScope scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetService<IMongoDatabase>();

            await asyncAction(db);
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
                return Host.CreateDefaultBuilder(null).ConfigureWebHostDefaults(webBuilder =>
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
