# MongoDB support for JsonApiDotNetCore

Plug-n-play implementation of `IResourceRepository<TResource, TId>` allowing you to use [MongoDB](https://www.mongodb.com/) with your [JsonApiDotNetCore](https://github.com/json-api-dotnet/JsonApiDotNetCore) APIs.

[![Build status](https://ci.appveyor.com/api/projects/status/dadm2kr2y0353mji/branch/master?svg=true)](https://ci.appveyor.com/project/json-api-dotnet/jsonapidotnetcore-mongodb/branch/master)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.MongoDb.svg)](https://www.nuget.org/packages/JsonApiDotNetCore.MongoDb/)

## Installation and Usage

```bash
dotnet add package JsonApiDotNetCore.MongoDb
```

### Models

```cs
public sealed class Book : IIdentifiable<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Attr]
    public string Id { get; set; }

    [Attr]
    public string Name { get; set; }

    [BsonIgnore]
    public string StringId { get => Id; set => Id = value; }
}
```

### Controllers

```cs
public sealed class BooksController : JsonApiController<Book, string>
{
    public BooksController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Book, string> resourceService)
        : base(options, loggerFactory, resourceService)
    {
    }
}
```

### Middleware

```cs
public class Startup
{
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
            return client.GetDatabase(Configuration.GetSection("DatabaseSettings:Database").Value);
        });

        services.AddResourceRepository<MongoDbRepository<Book>>();
        
        services.AddJsonApi(resources: builder =>
        {
            builder.Add<Book, string>();
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```


## Development

Restore all NuGet packages with:

```bash
dotnet restore
```

### Testing

You don't need to have a running instance of MongoDB on your machine. To run the tests just type the following command in your terminal:

```bash
dotnet test
```

If you want to run the examples and explore them on your own **you are** going to need that running instance of MongoDB. If you have docker installed you can launch it like this:

```bash
docker run -p 27017:27017 -d mongo:latest
```

And then to run the API:

```bash
dotnet run
```

## Limitations

- Relationships are not supported
