using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.Meta;

public sealed class TopLevelCountTests : IClassFixture<IntegrationTestContext<TestableStartup, MetaDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, MetaDbContext> _testContext;
    private readonly MetaFakers _fakers = new();

    public TopLevelCountTests(IntegrationTestContext<TestableStartup, MetaDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(SupportTicket).Namespace);

        testContext.UseController<SupportTicketsController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;
    }

    [Fact]
    public async Task Renders_resource_count_for_collection()
    {
        // Arrange
        SupportTicket ticket = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<SupportTicket>();
            dbContext.SupportTickets.Add(ticket);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Renders_resource_count_for_empty_collection()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext => await dbContext.ClearTableAsync<SupportTicket>());

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().ContainTotal(0);
    }

    [Fact]
    public async Task Hides_resource_count_in_create_resource_response()
    {
        // Arrange
        string newDescription = _fakers.SupportTicket.GenerateOne().Description;

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                attributes = new
                {
                    description = newDescription
                }
            }
        };

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Meta.Should().BeNull();
    }

    [Fact]
    public async Task Hides_resource_count_in_update_resource_response()
    {
        // Arrange
        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        string newDescription = _fakers.SupportTicket.GenerateOne().Description;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.Add(existingTicket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                id = existingTicket.StringId,
                attributes = new
                {
                    description = newDescription
                }
            }
        };

        string route = $"/supportTickets/{existingTicket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().BeNull();
    }
}
