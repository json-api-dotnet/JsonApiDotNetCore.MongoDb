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
public sealed class Book : MongoDbIdentifiable
{
    [Attr]
    public string Name { get; set; }
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

        services.AddJsonApi(resources: builder =>
        {
            builder.Add<Book, string>();
        });
        services.AddJsonApiMongoDb();

        services.AddResourceRepository<MongoDbRepository<Book, string>>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```

Note: If your API project uses only MongoDB (not in combination with EF Core), then instead of
registering all MongoDB resources and repositories individually, you can use:

```cs
public class Startup
{
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        // ...

        services.AddJsonApi(facade => facade.AddCurrentAssembly());
        services.AddJsonApiMongoDb();

        services.AddScoped(typeof(IResourceReadRepository<>), typeof(MongoDbRepository<>));
        services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoDbRepository<,>));
        services.AddScoped(typeof(IResourceWriteRepository<>), typeof(MongoDbRepository<>));
        services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoDbRepository<,>));
        services.AddScoped(typeof(IResourceRepository<>), typeof(MongoDbRepository<>));
        services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoDbRepository<,>));
    }
}
```

### Customise MongoDB persistence options and _id generation

`MongoDbIdentifiable` has some sensible defaults for storing documents with _ids, but these need to be customised and overridden if you want client side or string based ids.

For example, you could change the example above so that the `Book` resource has string IDs rather than object ids in the DB, (so far still generated server side).

Resources properties can use any of usual [MongoDB Driver mapping code](https://mongodb.github.io/mongo-csharp-driver/2.12/reference/bson/mapping/) so to achieve string based ids you have to [override the json:api resource class attributes](https://mongodb.github.io/mongo-csharp-driver/2.11/reference/bson/mapping/) using `BsonClassMap`:

```cs
    // in startup change to string generated ids for MongoDbIdentifiable
    BsonClassMap.RegisterClassMap<MongoDbIdentifiable>(cm =>
    {
        cm.MapIdProperty(x => x.Id)
            .SetIdGenerator(StringObjectIdGenerator.Instance);
    });

    // optionally you can also change the mapping for resources here
    BsonClassMap.RegisterClassMap<Book>(cm =>
    {
        cm.AutoMap();
        cm.MapProperty(x => x.Name).SetElementName("bookName");
    });
```

The `StringObjectIdGenerator` above can then be combined with `AllowClientGeneratedIds` JsonApi setting in `Startup.ConfigureServices` so that string IDs can be generated on the client, but will be auto-assigned to random strings server side if not provided. This style of ids will be more familiar to developers used to no-sql style databases.

```cs
    services.AddJsonApi(options => {
        // Allow us to POST books with already assigned IDs
        options.AllowClientGeneratedIds = true;
    }, resources: builder =>
    {
        builder.Add<Book, string>();
    });
    services.AddJsonApiMongoDb();

    services.AddResourceRepository<MongoDbRepository<Book, string>>();
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
