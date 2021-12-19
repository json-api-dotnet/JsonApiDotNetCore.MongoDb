# MongoDB support for JsonApiDotNetCore

Plug-n-play implementation of `IResourceRepository<TResource, TId>` allowing you to use [MongoDB](https://www.mongodb.com/) with your [JsonApiDotNetCore](https://github.com/json-api-dotnet/JsonApiDotNetCore) APIs.

[![Build](https://ci.appveyor.com/api/projects/status/dadm2kr2y0353mji/branch/master?svg=true)](https://ci.appveyor.com/project/json-api-dotnet/jsonapidotnetcore-mongodb/branch/master)
[![Coverage](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore.MongoDb/branch/master/graph/badge.svg?token=QPVf8rii7l)](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore.MongoDb)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.MongoDb.svg)](https://www.nuget.org/packages/JsonApiDotNetCore.MongoDb/)

## Installation and Usage

```bash
dotnet add package JsonApiDotNetCore.MongoDb
```

### Models

```c#
#nullable enable

[Resource]
public class Book : MongoIdentifiable
{
    [Attr]
    public string Name { get; set; } = null!;
}
```

### Middleware

```c#
// Program.cs

#nullable enable

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IMongoDatabase>(_ =>
{
    var client = new MongoClient("mongodb://localhost:27017");
    return client.GetDatabase("ExampleDbName");
});

builder.Services.AddJsonApi(resources: resourceGraphBuilder =>
{
    resourceGraphBuilder.Add<Book, string?>();
});

builder.Services.AddJsonApiMongoDb();

builder.Services.AddResourceRepository<MongoRepository<Book, string?>>();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

app.Run();
```

Note: If your API project uses MongoDB only (so not in combination with EF Core), then instead of
registering all MongoDB resources and repositories individually, you can use:

```c#
builder.Services.AddJsonApi(facade => facade.AddCurrentAssembly());
builder.Services.AddJsonApiMongoDb();

builder.Services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
builder.Services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
builder.Services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

```

## Limitations

- JSON:API relationships are currently not supported. You can use complex object graphs though, which are stored in a single document.

## Contributing

Have a question, found a bug or want to submit code changes? See our [contributing guidelines](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/.github/CONTRIBUTING.md).

## Trying out the latest build

After each commit to the master branch, a new prerelease NuGet package is automatically published to AppVeyor at https://ci.appveyor.com/nuget/jsonapidotnetcore-mongodb. To try it out, follow the next steps:

* In Visual Studio: **Tools**, **NuGet Package Manager**, **Package Manager Settings**, **Package Sources**
    * Click **+**
    * Name: **AppVeyor JADNC MongoDb**, Source: **https://ci.appveyor.com/nuget/jsonapidotnetcore-mongodb**
    * Click **Update**, **Ok**
* Open the NuGet package manager console (**Tools**, **NuGet Package Manager**, **Package Manager Console**)
    * Select **AppVeyor JADNC MongoDb** as package source
    * Run command: `Install-Package JonApiDotNetCore -pre`

## Development

To build the code from this repository locally, run:

```bash
dotnet build
```

You don't need to have a running instance of MongoDB on your machine to run tests. Just type the following command in your terminal:

```bash
dotnet test
```

If you want to run the examples and explore them on your own **you are** going to need that running instance of MongoDB. If you have docker installed you can launch it like this:

```bash
run-docker-mongodb.ps1
```

And then to run the API:

```bash
dotnet run --project src/Examples/GettingStarted
```

Alternatively, to build and validate the code, run all tests, generate code coverage and produce the NuGet package:

```bash
Build.ps1
```
