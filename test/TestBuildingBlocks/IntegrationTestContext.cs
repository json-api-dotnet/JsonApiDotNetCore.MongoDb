using System.Reflection;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    private readonly HashSet<Type> _resourceClrTypes = [];
    private readonly TestControllerProvider _testControllerProvider = new();
    private Action<IServiceCollection>? _configureServices;

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

    private static IMongoRunner StartMongoDb()
    {
        return MongoRunnerProvider.Instance.Get();
    }

    public void UseResourceTypesInNamespace(string? codeNamespace)
    {
        Assembly assembly = typeof(TStartup).Assembly;

        foreach (Type resourceClrType in ResourceTypeFinder.GetResourceClrTypesInNamespace(assembly, codeNamespace))
        {
            _resourceClrTypes.Add(resourceClrType);
        }
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

        factory.ConfigureServices(services =>
        {
            _configureServices?.Invoke(services);

            services.ReplaceControllers(_testControllerProvider);

            services.TryAddSingleton(_ =>
            {
                var client = new MongoClient(_runner.Value.ConnectionString);
                return client.GetDatabase($"JsonApiDotNetCore_MongoDb_{Random.Shared.Next()}_Test");
            });

            services.TryAddScoped<TMongoDbContextShim>();

            services.TryAddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
            services.TryAddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
            services.TryAddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

            services.AddJsonApi(ConfigureJsonApiOptions, resources: builder =>
            {
                foreach (Type resourceClrType in _resourceClrTypes)
                {
                    builder.Add(resourceClrType);
                }
            });

            services.AddJsonApiMongoDb();
        });

        return factory;
    }

    private static void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        options.IncludeExceptionStackTraceInErrors = true;
        options.IncludeRequestBodyInErrors = true;
        options.SerializerOptions.WriteIndented = true;
    }

    public void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        if (_configureServices != null && _configureServices != configureServices)
        {
            throw new InvalidOperationException($"Do not call {nameof(ConfigureServices)} multiple times.");
        }

        _configureServices = configureServices;
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
        private Action<IServiceCollection>? _configureServices;

        public void ConfigureServices(Action<IServiceCollection>? configureServices)
        {
            _configureServices = configureServices;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // We have placed an appsettings.json in the TestBuildingBlocks project directory and set the content root to there. Note that
            // controllers are not discovered in the content root, but are registered manually using IntegrationTestContext.UseController.
            builder.UseSolutionRelativeContentRoot($"test/{nameof(TestBuildingBlocks)}");
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_before_first_method_call true

            return Host
                .CreateDefaultBuilder(null)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services => _configureServices?.Invoke(services));
                    webBuilder.UseStartup<TStartup>();
                });

            // @formatter:wrap_before_first_method_call restore
            // @formatter:wrap_chained_method_calls restore
        }
    }
}
