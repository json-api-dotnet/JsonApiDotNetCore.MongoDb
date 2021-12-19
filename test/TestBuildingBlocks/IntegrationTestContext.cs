using System.Runtime.InteropServices;
using System.Text.Json;
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
using Mongo2Go;
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
public class IntegrationTestContext<TStartup, TMongoDbContextShim> : IntegrationTest, IDisposable
    where TStartup : class
    where TMongoDbContextShim : MongoDbContextShim
{
    private readonly Lazy<MongoDbRunner> _runner;
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

    /// <summary>
    /// Set this to <c>true</c> to enable transactions support in MongoDB.
    /// </summary>
    public bool StartMongoDbInSingleNodeReplicaSetMode { get; set; }

    public IntegrationTestContext()
    {
        _runner = new Lazy<MongoDbRunner>(StartMongoDb);
        _lazyFactory = new Lazy<WebApplicationFactory<TStartup>>(CreateFactory);
    }

    private MongoDbRunner StartMongoDb()
    {
        // Increasing maxTransactionLockRequestTimeoutMillis (default=5) as workaround for occasional
        // "Unable to acquire lock" error when running tests locally.
        string arguments = "--quiet --setParameter maxTransactionLockRequestTimeoutMillis=40";

        if (!StartMongoDbInSingleNodeReplicaSetMode)
        {
            // MongoDbRunner watches console output to detect when the replica set has stabilized. So we can only fully
            // suppress console output if not running in this mode.
            arguments += RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? " --logappend --logpath NUL" : " --logpath /dev/null";
        }

        return MongoDbRunner.Start(singleNodeReplSet: StartMongoDbInSingleNodeReplicaSetMode, additionalMongodArguments: arguments);
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

    public void Dispose()
    {
        if (_lazyFactory.IsValueCreated)
        {
            _lazyFactory.Value.Dispose();
        }

        if (_runner.IsValueCreated)
        {
            _runner.Value.Dispose();
        }
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
            // @formatter:keep_existing_linebreaks true

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

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore
        }
    }
}
