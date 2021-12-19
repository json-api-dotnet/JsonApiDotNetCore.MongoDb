using System.Text.Json.Serialization;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<ISystemClock, SystemClock>();

builder.Services.AddSingleton(_ =>
{
    var client = new MongoClient(builder.Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
    return client.GetDatabase(builder.Configuration.GetSection("DatabaseSettings:Database").Value);
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
    options.Namespace = "api/v1";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
#if DEBUG
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
#endif
}
