# JsonApiDotNetCore MongoDB Repository

[![Build Status](https://travis-ci.com/mrnkr/JsonApiDotNetCore.MongoDb.svg?branch=master)](https://travis-ci.com/mrnkr/JsonApiDotNetCore.MongoDb)
[![codecov](https://codecov.io/gh/mrnkr/JsonApiDotNetCore.MongoDb/branch/master/graph/badge.svg)](https://codecov.io/gh/mrnkr/JsonApiDotNetCore.MongoDb)

Plug-n-play implementation of `IResourceRepository<TResource, TId>` allowing you to use MongoDb with your `JsonApiDotNetCore` APIs.

## Installation and Usage

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
    public IServiceProvider ConfigureServices(IServiceCollection services) {
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
            return client.GetDatabase(Configuration.GetSection("DatabaseSettings:Database").Value);
        });

        services.AddScoped<IResourceRepository<Book, string>, MongoEntityRepository<Book, string>>();
        services.AddJsonApi(options =>
        {
            options.Namespace = "api";
            options.UseRelativeLinks = true;
            options.IncludeTotalResourceCount = true;
            options.SerializerSettings.Formatting = Formatting.Indented;
        }, resources: builder =>
        {
            builder.Add<Book, string>();
        });
        // ...
    }

    public void Configure(IApplicationBuilder app)  {
        app.UseJsonApi();
        // ...
    }
}
```

## Limitations

- Relations are not supported (yet)
- Projections are not supported (yet)
