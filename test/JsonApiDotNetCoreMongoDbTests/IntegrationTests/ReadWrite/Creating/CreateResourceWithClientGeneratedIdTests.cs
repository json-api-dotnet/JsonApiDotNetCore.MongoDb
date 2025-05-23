using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Creating;

public sealed class CreateResourceWithClientGeneratedIdTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public CreateResourceWithClientGeneratedIdTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(WorkItem).Namespace);

        testContext.UseController<WorkItemGroupsController>();
        testContext.UseController<RgbColorsController>();

        testContext.ConfigureServices(services => services.AddResourceDefinition<ImplicitlyChangingWorkItemGroupDefinition>());

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = ClientIdGenerationMode.Required;
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_string_ID_having_side_effects()
    {
        // Arrange
        WorkItemGroup newGroup = _fakers.WorkItemGroup.GenerateOne();
        newGroup.Id = "free-format-client-generated-id-1";

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = newGroup.StringId,
                attributes = new
                {
                    name = newGroup.Name
                }
            }
        };

        const string route = "/workItemGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        string groupName = $"{newGroup.Name}{ImplicitlyChangingWorkItemGroupDefinition.Suffix}";

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItemGroups");
        responseDocument.Data.SingleValue.Id.Should().Be(newGroup.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(groupName);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItemGroup groupInDatabase = await dbContext.Groups.FirstWithIdAsync(newGroup.Id);

            groupInDatabase.Name.Should().Be(groupName);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_string_ID_having_side_effects_with_fieldset()
    {
        // Arrange
        WorkItemGroup newGroup = _fakers.WorkItemGroup.GenerateOne();
        newGroup.Id = "free-format-client-generated-id-2";

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = newGroup.StringId,
                attributes = new
                {
                    name = newGroup.Name
                }
            }
        };

        const string route = "/workItemGroups?fields[workItemGroups]=name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        string groupName = $"{newGroup.Name}{ImplicitlyChangingWorkItemGroupDefinition.Suffix}";

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItemGroups");
        responseDocument.Data.SingleValue.Id.Should().Be(newGroup.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(groupName);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItemGroup groupInDatabase = await dbContext.Groups.FirstWithIdAsync(newGroup.Id);

            groupInDatabase.Name.Should().Be(groupName);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
    {
        // Arrange
        RgbColor newColor = _fakers.RgbColor.GenerateOne();
        newColor.Id = "free-format-client-generated-id-3";

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = newColor.StringId,
                attributes = new
                {
                    displayName = newColor.DisplayName
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.FirstWithIdAsync(newColor.Id);

            colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects_with_fieldset()
    {
        // Arrange
        RgbColor newColor = _fakers.RgbColor.GenerateOne();
        newColor.Id = "free-format-client-generated-id-4";

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = newColor.StringId,
                attributes = new
                {
                    displayName = newColor.DisplayName
                }
            }
        };

        const string route = "/rgbColors?fields[rgbColors]=id";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.FirstWithIdAsync(newColor.Id);

            colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_for_existing_client_generated_ID()
    {
        // Arrange
        RgbColor existingColor = _fakers.RgbColor.GenerateOne();
        existingColor.Id = "free-format-client-generated-id-5";

        string newDisplayName = _fakers.RgbColor.GenerateOne().DisplayName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.RgbColors.Add(existingColor);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = existingColor.StringId,
                attributes = new
                {
                    displayName = newDisplayName
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'rgbColors' with ID '{existingColor.StringId}' already exists.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }
}
