using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using EphemeralMongo;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Xunit;

namespace TestBuildingBlocks;

/// <summary>
/// Base class for a test context that creates a new database and server instance before running tests and cleans up afterward. You can either use this
/// as a fixture on your tests class (init/cleanup runs once before/after all tests) or have your tests class inherit from it (init/cleanup runs once
/// before/after each test). See <see href="https://xunit.net/docs/shared-context" /> for details on shared context usage.
/// </summary>
/// <typeparam name="TStartup">
/// The Startup class that configures the server.
/// </typeparam>
/// <typeparam name="TMongoDbContextShim">
/// The <see cref="MongoDbContextShim" /> that defines the entity models.
/// </typeparam>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class IntegrationTestContext<TStartup, TMongoDbContextShim> : IntegrationTest, IAsyncLifetime
    where TStartup : IStartup, new()
    where TMongoDbContextShim : MongoDbContextShim
{
    private readonly Lazy<IMongoRunner> _runner;
    private readonly Lazy<WebApplication> _lazyApp;
    private readonly HashSet<Type> _resourceClrTypes = [];
    private readonly TestControllerProvider _testControllerProvider = new();
    private Action<IServiceCollection>? _configureServices;
    private bool _throttleAcquired;

    protected override JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = App.Services.GetRequiredService<IJsonApiOptions>();
            return options.SerializerOptions;
        }
    }

    public WebApplication App => _lazyApp.Value;

    public IntegrationTestContext()
    {
        _runner = new Lazy<IMongoRunner>(StartMongoDb);
        _lazyApp = new Lazy<WebApplication>(BuildApp, LazyThreadSafetyMode.ExecutionAndPublication);
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
        return App.GetTestClient();
    }

    private WebApplication BuildApp()
    {
        var startup = new TStartup();

        WebApplicationBuilder builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            ApplicationName = startup.GetType().Assembly.GetName().Name
        });

        _configureServices?.Invoke(builder.Services);
        startup.ConfigureServices(builder.Services);

        builder.Services.ReplaceControllers(_testControllerProvider);

        builder.Services.TryAddSingleton(_ =>
        {
            var client = new MongoClient(_runner.Value.ConnectionString);
            return client.GetDatabase($"JsonApiDotNetCore_MongoDb_{Random.Shared.Next()}_Test");
        });

        builder.Services.TryAddScoped<TMongoDbContextShim>();

        builder.Services.TryAddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
        builder.Services.TryAddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
        builder.Services.TryAddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

        builder.Services.AddJsonApi(ConfigureJsonApiOptions, resources: resourceGraphBuilder =>
        {
            foreach (Type resourceClrType in _resourceClrTypes)
            {
                resourceGraphBuilder.Add(resourceClrType);
            }
        });

        builder.Services.AddJsonApiMongoDb();

        ClearLoggingProvidersInReleaseBuild(builder.Logging);

        builder.Host.UseDefaultServiceProvider(ConfigureServiceProvider);
        builder.WebHost.UseTestServer();

        WebApplication app = builder.Build();
        startup.Configure(app);

        app.Start();

        return app;
    }

    private static void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        options.IncludeExceptionStackTraceInErrors = true;
        options.IncludeRequestBodyInErrors = true;
        options.SerializerOptions.WriteIndented = true;
    }

    [Conditional("RELEASE")]
    private static void ClearLoggingProvidersInReleaseBuild(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
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
        AssertThrottleAcquired();

        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        var mongoDbContextShim = scope.ServiceProvider.GetRequiredService<TMongoDbContextShim>();

        await asyncAction(mongoDbContextShim);
    }

    private void AssertThrottleAcquired()
    {
        if (!_throttleAcquired)
        {
            throw new InvalidOperationException("Database throttle not acquired.");
        }
    }

    public async Task InitializeAsync()
    {
        await AcquireDatabaseThrottleAsync();
        _throttleAcquired = true;
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_lazyApp.IsValueCreated)
            {
                await App.StopAsync();
                await App.DisposeAsync();
            }

            if (_runner.IsValueCreated)
            {
                _runner.Value.Dispose();
            }
        }
        finally
        {
            ReleaseDatabaseThrottle();
        }
    }
}
