using GettingStarted.Definitions;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Newtonsoft.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.TryAddSingleton(_ =>
{
    var client = new MongoClient(builder.Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
    return client.GetDatabase(builder.Configuration.GetSection("DatabaseSettings:Database").Value);
});

builder.Services.AddJsonApi(ConfigureJsonApiOptions, resources: resourceGraphBuilder => resourceGraphBuilder.Add<Book, string?>());
builder.Services.AddJsonApiMongoDb();

builder.Services.AddResourceRepository<MongoRepository<Book, string?>>();
builder.Services.AddResourceDefinition<BooksDefinition>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

var database = app.Services.GetRequiredService<IMongoDatabase>();
await CreateSampleDataAsync(database);

app.Run();

static void ConfigureJsonApiOptions(JsonApiOptions options)
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;
    options.SerializerSettings.Formatting = Formatting.Indented;
}

static async Task CreateSampleDataAsync(IMongoDatabase database)
{
    await database.DropCollectionAsync(nameof(Book));

    await database.GetCollection<Book>(nameof(Book)).InsertManyAsync(new[]
    {
        new Book
        {
            Title = "Frankenstein",
            PublishYear = 1818,
            Author = "Mary Shelley"
        },
        new Book
        {
            Title = "Robinson Crusoe",
            PublishYear = 1719,
            Author = "Daniel Defoe"
        },
        new Book
        {
            Title = "Gulliver's Travels",
            PublishYear = 1726,
            Author = "Jonathan Swift"
        }
    });
}
