# JsonApiDotNetCore MongoDB Repository

[![NuGet version][nuget-image]][nuget-url]
[![Downloads][downloads-image]][nuget-url]
[![Build Status](https://travis-ci.com/mrnkr/JsonApiDotNetCore.MongoDb.svg?branch=master)](https://travis-ci.com/mrnkr/JsonApiDotNetCore.MongoDb)
[![codecov](https://codecov.io/gh/mrnkr/JsonApiDotNetCore.MongoDb/branch/master/graph/badge.svg)](https://codecov.io/gh/mrnkr/JsonApiDotNetCore.MongoDb)
[![license][license]](https://github.com/mrnkr/JsonApiDotNetCore.MongoDb/blob/master/LICENSE)

[nuget-image]:https://img.shields.io/nuget/v/JsonApiDotNetCore.MongoDb
[nuget-url]:https://www.nuget.org/packages/JsonApiDotNetCore.MongoDb
[downloads-image]:https://img.shields.io/nuget/dt/JsonApiDotNetCore.MongoDb
[license]:https://img.shields.io/github/license/mrnkr/JsonApiDotNetCore.MongoDb

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
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
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

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        // ...
    }
}
```

## Running tests and examples

Integration tests use the [`Mongo2Go`](https://github.com/Mongo2Go/Mongo2Go) package so they don't require a running instance of MongoDb on your machine.

Just run the following command to run all tests:

```bash
dotnet test
```

To run the examples you are indeed going to want to have a running instance of MongoDb on your device. Fastest way to get one running is using docker:

```bash
docker run -p 27017:27017 -d mongo:latest
dotnet run
```

## Limitations

- Relations are not supported (yet)
