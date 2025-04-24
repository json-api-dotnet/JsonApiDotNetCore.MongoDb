# MongoDB support for JsonApiDotNetCore

[![Build](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/actions/workflows/build.yml?query=branch%3Amaster)
[![Coverage](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore.MongoDb/branch/master/graph/badge.svg?token=QPVf8rii7l)](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore.MongoDb)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.MongoDb.svg)](https://www.nuget.org/packages/JsonApiDotNetCore.MongoDb/)
[![GitHub License](https://img.shields.io/github/license/json-api-dotnet/JsonApiDotNetCore.MongoDb)](LICENSE)

Plug-n-play implementation of `IResourceRepository<TResource, TId>`, allowing you to use [MongoDB](https://www.mongodb.com/) with your [JsonApiDotNetCore](https://github.com/json-api-dotnet/JsonApiDotNetCore) API projects.

## Getting started

The following steps describe how to create a JSON:API project with MongoDB.

1. Install the JsonApiDotNetCore.MongoDb package:
   ```bash
   dotnet add package JsonApiDotNetCore.MongoDb
   ```

1. Declare your entities, annotated with JsonApiDotNetCore attributes:
   ```c#
   #nullable enable

   [Resource]
   public class Person : HexStringMongoIdentifiable
   {
       [Attr] public string? FirstName { get; set; }
       [Attr] public string LastName { get; set; } = null!;
   }
   ```

1. Configure MongoDB and JsonApiDotNetCore in `Program.cs`, seeding the database with sample data:
   ```c#
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddSingleton(_ => new MongoClient("mongodb://localhost:27017").GetDatabase("ExampleDbName"));
   builder.Services.AddJsonApi(options =>
   {
       options.UseRelativeLinks = true;
       options.IncludeTotalResourceCount = true;
   }, resources: resourceGraphBuilder => resourceGraphBuilder.Add<Person, string?>());
   builder.Services.AddJsonApiMongoDb();
   builder.Services.AddResourceRepository<MongoRepository<Person, string?>>();

   var app = builder.Build();
   app.UseRouting();
   app.UseJsonApi();
   app.MapControllers();

   var database = app.Services.GetRequiredService<IMongoDatabase>();
   await CreateSampleDataAsync(database);

   app.Run();

   static async Task CreateSampleDataAsync(IMongoDatabase database)
   {
       await database.DropCollectionAsync(nameof(Person));
       await database.GetCollection<Person>(nameof(Person)).InsertManyAsync(new[]
       {
           new Person
           {
               FirstName = "John",
               LastName = "Doe",
           },
           new Person
           {
               FirstName = "Jane",
               LastName = "Doe",
           },
           new Person
           {
               FirstName = "John",
               LastName = "Smith",
           }
       });
   }
   ```

   > [!TIP]
   > If your API project uses MongoDB only (so not in combination with EF Core), then instead of
   > registering all MongoDB resources and repositories individually, you can use:
   >
   > ```c#
   > builder.Services.AddJsonApi(facade => facade.AddCurrentAssembly());
   > builder.Services.AddJsonApiMongoDb();
   >
   > builder.Services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
   > builder.Services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
   > builder.Services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));
   > ```

1. Start your API
   ```bash
   dotnet run
   ```

1. Send a GET request to retrieve data:
   ```bash
   GET http://localhost:5000/people?filter=equals(lastName,'Doe')&fields[people]=firstName HTTP/1.1
   ```

   <details>
   <summary>Expand to view the JSON response</summary>

   ```json
   {
     "links": {
       "self": "/people?filter=equals(lastName,%27Doe%27)&fields[people]=firstName",
       "first": "/people?filter=equals(lastName,%27Doe%27)&fields%5Bpeople%5D=firstName",
       "last": "/people?filter=equals(lastName,%27Doe%27)&fields%5Bpeople%5D=firstName"
     },
     "data": [
       {
         "type": "people",
         "id": "680cae2e1759666c5c1e988c",
         "attributes": {
           "firstName": "John"
         },
         "links": {
           "self": "/people/680cae2e1759666c5c1e988c"
         }
       },
       {
         "type": "people",
         "id": "680cae2e1759666c5c1e988d",
         "attributes": {
           "firstName": "Jane"
         },
         "links": {
           "self": "/people/680cae2e1759666c5c1e988d"
         }
       }
     ],
     "meta": {
       "total": 2
     }
   }
   ```

</details>

## Using client-generated IDs

Resources that inherit from `HexStringMongoIdentifiable` use auto-generated (high-performance) 12-byte hexadecimal
[Object IDs](https://docs.mongodb.com/manual/reference/bson-types/#objectid).
You can assign an ID explicitly, but it must match the 12-byte hexadecimal pattern.

To use free-format string IDs, make your resources inherit from `FreeStringMongoIdentifiable` instead.
When creating a resource without assigning an ID, a 12-byte hexadecimal ID will be auto-generated.

Set `options.ClientIdGeneration` to `Allowed` or `Required` from `Program.cs` to enable API clients to assign IDs. This can be combined
with both base classes, but `FreeStringMongoIdentifiable` probably makes the most sense.

## Limitations

- JSON:API relationships are [currently not supported](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/issues/73). You *can* use complex object graphs though, which are stored in a single document.

## Trying out the latest build

After each commit to the master branch, a new pre-release NuGet package is automatically published to [feedz.io](https://feedz.io/docs/package-types/nuget).
To try it out, follow the steps below:

1. Create a `nuget.config` file in the same directory as your .sln file, with the following contents:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="json-api-dotnet" value="https://f.feedz.io/json-api-dotnet/jsonapidotnetcore/nuget/index.json" />
       <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
     </packageSources>
   </configuration>
   ```

1. In your IDE, browse the list of packages from the `json-api-dotnet` feed. Make sure pre-release packages are included in the list.

## Contributing

Have a question, found a bug or want to submit code changes? See our [contributing guidelines](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/.github/CONTRIBUTING.md).

## Build from source

To build the code from this repository locally, run:

```bash
dotnet build
```

You can run tests without MongoDB on your machine. The following command runs all tests:

```bash
dotnet test
```

A running instance of MongoDB is required to run the examples.
If you have docker installed, you can launch MongoDB in a container with the following command:

```bash
pwsh run-docker-mongodb.ps1
```

And then to run the API:

```bash
dotnet run --project src/Examples/GettingStarted
```

Alternatively, to build, run all tests, generate code coverage and NuGet packages:

```bash
pwsh Build.ps1
```
