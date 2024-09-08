# MongoDB support for JsonApiDotNetCore

Plug-n-play implementation of `IResourceRepository<TResource, TId>` allowing you to use [MongoDB](https://www.mongodb.com/) with your [JsonApiDotNetCore](https://github.com/json-api-dotnet/JsonApiDotNetCore) APIs.

[![Build](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/actions/workflows/build.yml?query=branch%3Amaster)
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
public class Book : HexStringMongoIdentifiable
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

## Using client-generated IDs
Resources that inherit from `HexStringMongoIdentifiable` use auto-generated (performant) 12-byte hexadecimal
[Object IDs](https://docs.mongodb.com/manual/reference/bson-types/#objectid).
You can assign an ID manually, but it must match the 12-byte hexadecimal pattern.

To assign free-format string IDs manually, make your resources inherit from `FreeStringMongoIdentifiable` instead.
When creating a resource without assigning an ID, a 12-byte hexadecimal ID will be auto-generated.

Set `options.AllowClientGeneratedIds` to `true` in Program.cs to allow API clients to assign IDs. This can be combined
with both base classes, but `FreeStringMongoIdentifiable` probably makes the most sense.

## Limitations

- JSON:API relationships are [currently not supported](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/issues/73). You can use complex object graphs though, which are stored in a single document.

## Contributing

Have a question, found a bug or want to submit code changes? See our [contributing guidelines](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/.github/CONTRIBUTING.md).

## Trying out the latest build

After each commit to the master branch, a new pre-release NuGet package is automatically published to [GitHub Packages](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).
To try it out, follow the steps below:

1. [Create a Personal Access Token (classic)](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic) with at least `read:packages` scope.
1. Add our package source to your local user-specific `nuget.config` file by running:
   ```bash
   dotnet nuget add source https://nuget.pkg.github.com/json-api-dotnet/index.json --name github-json-api --username YOUR-GITHUB-USERNAME --password YOUR-PAT-CLASSIC
   ```
   In the command above:
   - Replace YOUR-GITHUB-USERNAME with the username you use to login your GitHub account.
   - Replace YOUR-PAT-CLASSIC with the token your created above.
   
   :warning: If the above command doesn't give you access in the next step, remove the package source by running:
   ```bash
   dotnet nuget remove source github-json-api
   ```
   and retry with the `--store-password-in-clear-text` switch added.
1. Restart your IDE, open your project, and browse the list of packages from the github-json-api feed (make sure pre-release packages are included).

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
