using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
#if NET6_0
using Microsoft.AspNetCore.Authentication;
#endif

[assembly: ExcludeFromCodeCoverage]

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#if NET6_0
builder.Services.TryAddSingleton<ISystemClock, SystemClock>();
#else
builder.Services.TryAddSingleton(TimeProvider.System);
#endif

builder.Services.AddSingleton(_ =>
{
    var client = new MongoClient(builder.Configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
    return client.GetDatabase(builder.Configuration.GetValue<string>("DatabaseSettings:Database"));
});

builder.Services.AddJsonApi(ConfigureJsonApiOptions, facade => facade.AddCurrentAssembly());
builder.Services.AddJsonApiMongoDb();

builder.Services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
builder.Services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
builder.Services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

app.Run();

static void ConfigureJsonApiOptions(JsonApiOptions options)
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
#if DEBUG
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
    options.SerializerOptions.WriteIndented = true;
#endif
}
