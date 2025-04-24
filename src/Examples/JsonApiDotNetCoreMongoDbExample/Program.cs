using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

[assembly: ExcludeFromCodeCoverage]

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.TryAddSingleton(TimeProvider.System);

builder.Services.TryAddSingleton(_ =>
{
    var client = new MongoClient(builder.Configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
    return client.GetDatabase(builder.Configuration.GetValue<string>("DatabaseSettings:Database"));
});

builder.Services.TryAddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
builder.Services.TryAddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
builder.Services.TryAddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

builder.Services.AddJsonApi(ConfigureJsonApiOptions, facade => facade.AddCurrentAssembly());
builder.Services.AddJsonApiMongoDb();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await app.RunAsync();

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
