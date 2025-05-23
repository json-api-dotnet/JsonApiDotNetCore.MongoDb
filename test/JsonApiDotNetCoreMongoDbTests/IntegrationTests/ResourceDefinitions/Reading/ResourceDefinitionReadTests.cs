using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

public sealed class ResourceDefinitionReadTests : IClassFixture<IntegrationTestContext<TestableStartup, UniverseDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, UniverseDbContext> _testContext;
    private readonly UniverseFakers _fakers = new();

    public ResourceDefinitionReadTests(IntegrationTestContext<TestableStartup, UniverseDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(Star).Namespace);

        testContext.UseController<StarsController>();
        testContext.UseController<PlanetsController>();
        testContext.UseController<MoonsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<StarDefinition>();
            services.AddResourceDefinition<PlanetDefinition>();
            services.AddResourceDefinition<MoonDefinition>();

            services.AddSingleton<IClientSettingsProvider, TestClientSettingsProvider>();
            services.AddSingleton<ResourceDefinitionHitCounter>();
        });

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;

        var settingsProvider = (TestClientSettingsProvider)testContext.Factory.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.ResetToDefaults();

        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();
        hitCounter.Reset();
    }

    [Fact]
    public async Task Filter_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.Factory.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HidePlanetsWithPrivateName();

        List<Planet> planets = _fakers.Planet.GenerateList(4);
        planets[0].PrivateName = "A";
        planets[2].PrivateName = "B";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Planet>();
            dbContext.Planets.AddRange(planets);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(planets[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(planets[3].StringId);

        responseDocument.Meta.Should().ContainTotal(2);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Filter_from_resource_definition_and_query_string_are_applied()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.Factory.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HidePlanetsWithPrivateName();

        List<Planet> planets = _fakers.Planet.GenerateList(4);

        planets[0].HasRingSystem = true;
        planets[0].PrivateName = "A";

        planets[1].HasRingSystem = false;
        planets[1].PrivateName = "B";

        planets[2].HasRingSystem = true;

        planets[3].HasRingSystem = false;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Planet>();
            dbContext.Planets.AddRange(planets);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/planets?filter=equals(hasRingSystem,'false')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(planets[3].StringId);

        responseDocument.Meta.Should().ContainTotal(1);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Sort_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Star> stars = _fakers.Star.GenerateList(3);

        stars[0].SolarMass = 500m;
        stars[0].SolarRadius = 1m;

        stars[1].SolarMass = 500m;
        stars[1].SolarRadius = 10m;

        stars[2].SolarMass = 50m;
        stars[2].SolarRadius = 15m;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.AddRange(stars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/stars";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(stars[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(stars[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(stars[2].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Sort_from_query_string_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Star> stars = _fakers.Star.GenerateList(3);

        stars[0].Name = "B";
        stars[0].SolarRadius = 10m;

        stars[1].Name = "B";
        stars[1].SolarRadius = 1m;

        stars[2].Name = "A";
        stars[2].SolarRadius = 15m;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.AddRange(stars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/stars?sort=name,-solarRadius";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(stars[2].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(stars[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(stars[1].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Page_size_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Star> stars = _fakers.Star.GenerateList(10);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.AddRange(stars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/stars?page[size]=8";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(5);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_inclusion_from_resource_definition_is_applied_for_omitted_query_string()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("kind").WhoseValue.Should().Be(star.Kind);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_inclusion_from_resource_definition_is_applied_for_fields_query_string()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}?fields[stars]=name,solarRadius";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(2);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("solarRadius").WhoseValue.Should().Be(star.SolarRadius);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_exclusion_from_resource_definition_is_applied_for_omitted_query_string()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("isVisibleFromEarth");
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_exclusion_from_resource_definition_is_applied_for_fields_query_string()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}?fields[stars]=name,isVisibleFromEarth";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Queryable_parameter_handler_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Moon> moons = _fakers.Moon.GenerateList(2);
        moons[0].SolarRadius = .5m;
        moons[1].SolarRadius = 50m;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Moon>();
            dbContext.Moons.AddRange(moons);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/moons?isLargerThanTheSun=true";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(moons[1].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Queryable_parameter_handler_from_resource_definition_and_query_string_filter_are_applied()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Moon> moons = _fakers.Moon.GenerateList(4);

        moons[0].Name = "Alpha1";
        moons[0].SolarRadius = 1m;

        moons[1].Name = "Alpha2";
        moons[1].SolarRadius = 5m;

        moons[2].Name = "Beta1";
        moons[2].SolarRadius = 1m;

        moons[3].Name = "Beta2";
        moons[3].SolarRadius = 5m;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Moon>();
            dbContext.Moons.AddRange(moons);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/moons?isLargerThanTheSun=false&filter=startsWith(name,'B')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(moons[2].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Queryable_parameter_handler_from_resource_definition_is_not_applied_on_secondary_request()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Planet planet = _fakers.Planet.GenerateOne();
        planet.Moons = _fakers.Moon.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/planets/{planet.StringId}/moons?isLargerThanTheSun=false";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Custom query string parameters cannot be used on nested resource endpoints.");
        error.Detail.Should().Be("Query string parameter 'isLargerThanTheSun' cannot be used on a nested resource endpoint.");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("isLargerThanTheSun");

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters)
        }, options => options.WithStrictOrdering());
    }
}
