using System.Text.Json;
using EphemeralMongo;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace TestBuildingBlocks;

/// <summary>
/// Base class for a test context that creates a new database and server instance before running tests and cleans up afterwards. You can either use this
/// as a fixture on your tests class (init/cleanup runs once before/after all tests) or have your tests class inherit from it (init/cleanup runs once
/// before/after each test). See <see href="https://xunit.net/docs/shared-context" /> for details on shared context usage.
/// </summary>
/// <typeparam name="TStartup">
/// The server Startup class, which can be defined in the test project or API project.
/// </typeparam>
/// <typeparam name="TMongoDbContextShim">
/// <see cref="MongoDbContextShim" />.
/// </typeparam>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class IntegrationTestContext<TStartup, TMongoDbContextShim> : IntegrationTest
    where TStartup : class
    where TMongoDbContextShim : MongoDbContextShim
{
    private readonly Lazy<IMongoRunner> _runner;
    private readonly Lazy<WebApplicationFactory<TStartup>> _lazyFactory;
    private readonly TestControllerProvider _testControllerProvider = new();

    private Action<IServiceCollection>? _afterServicesConfiguration;

    protected override JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = Factory.Services.GetRequiredService<IJsonApiOptions>();
            return options.SerializerOptions;
        }
    }

    public WebApplicationFactory<TStartup> Factory => _lazyFactory.Value;

    public IntegrationTestContext()
    {
        _runner = new Lazy<IMongoRunner>(StartMongoDb);
        _lazyFactory = new Lazy<WebApplicationFactory<TStartup>>(CreateFactory);
    }

    private IMongoRunner StartMongoDb()
    {
        return MongoRunnerProvider.Instance.Get();
    }

    public void UseController<TController>()
        where TController : ControllerBase
    {
        _testControllerProvider.AddController(typeof(TController));
    }

    protected override HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    private WebApplicationFactory<TStartup> CreateFactory()
    {
        var factory = new IntegrationTestWebApplicationFactory();

        factory.ConfigureServicesBeforeStartup(services =>
        {
            services.ReplaceControllers(_testControllerProvider);

            services.AddSingleton(_ =>
            {
                var client = new MongoClient(_runner.Value.ConnectionString);
                return client.GetDatabase($"JsonApiDotNetCore_MongoDb_{new Random().Next()}_Test");
            });

            services.AddJsonApi(ConfigureJsonApiOptions, facade => facade.AddAssembly(typeof(TStartup).Assembly));
            services.AddJsonApiMongoDb();

            services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
            services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
            services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

            services.AddScoped<TMongoDbContextShim>();
        });

        factory.ConfigureServicesAfterStartup(_afterServicesConfiguration);

        // We have placed an appsettings.json in the TestBuildingBlock project folder and set the content root to there. Note that controllers
        // are not discovered in the content root but are registered manually using IntegrationTestContext.UseController.
        return factory.WithWebHostBuilder(builder => builder.UseSolutionRelativeContentRoot($"test/{nameof(TestBuildingBlocks)}"));
    }

    private void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        options.IncludeExceptionStackTraceInErrors = true;
        options.IncludeRequestBodyInErrors = true;
        options.SerializerOptions.WriteIndented = true;
    }

    public void ConfigureServicesAfterStartup(Action<IServiceCollection> servicesConfiguration)
    {
        _afterServicesConfiguration = servicesConfiguration;
    }

    public async Task RunOnDatabaseAsync(Func<TMongoDbContextShim, Task> asyncAction)
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        var mongoDbContextShim = scope.ServiceProvider.GetRequiredService<TMongoDbContextShim>();

        await asyncAction(mongoDbContextShim);
    }

    public override async Task DisposeAsync()
    {
        try
        {
            if (_lazyFactory.IsValueCreated)
            {
                await _lazyFactory.Value.DisposeAsync();
            }

            if (_runner.IsValueCreated)
            {
                _runner.Value.Dispose();
            }
        }
        finally
        {
            await base.DisposeAsync();
        }
    }

    private sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<TStartup>
    {
        private Action<IServiceCollection>? _beforeServicesConfiguration;
        private Action<IServiceCollection>? _afterServicesConfiguration;

        public void ConfigureServicesBeforeStartup(Action<IServiceCollection>? servicesConfiguration)
        {
            _beforeServicesConfiguration = servicesConfiguration;
        }

        public void ConfigureServicesAfterStartup(Action<IServiceCollection>? servicesConfiguration)
        {
            _afterServicesConfiguration = servicesConfiguration;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_before_first_method_call true

            return Host
                .CreateDefaultBuilder(null)
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

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_before_first_method_call restore
        }
    }
}
