using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Updating.Resources;

public sealed class UpdateResourceTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public UpdateResourceTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<WorkItemGroupsController>();
        testContext.UseController<UserAccountsController>();
        testContext.UseController<RgbColorsController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddResourceDefinition<ContainerTypeToHideFromAutoDiscovery.ImplicitlyChangingWorkItemDefinition>();
            services.AddResourceDefinition<ContainerTypeToHideFromAutoDiscovery.ImplicitlyChangingWorkItemGroupDefinition>();
        });
    }

    [Fact]
    public async Task Can_update_resource_without_attributes_or_relationships()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(existingUserAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = existingUserAccount.StringId,
                attributes = new
                {
                },
                relationships = new
                {
                }
            }
        };

        string route = $"/userAccounts/{existingUserAccount.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            UserAccount userAccountInDatabase = await dbContext.UserAccounts.FirstWithIdAsync(existingUserAccount.Id);

            userAccountInDatabase.FirstName.Should().Be(existingUserAccount.FirstName);
            userAccountInDatabase.LastName.Should().Be(existingUserAccount.LastName);
        });
    }

    [Fact]
    public async Task Can_partially_update_resource_with_string_ID()
    {
        // Arrange
        WorkItemGroup existingGroup = _fakers.WorkItemGroup.Generate();
        string newName = _fakers.WorkItemGroup.Generate().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Groups.Add(existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = existingGroup.StringId,
                attributes = new
                {
                    name = newName
                }
            }
        };

        string route = $"/workItemGroups/{existingGroup.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        string groupName = $"{newName}{ContainerTypeToHideFromAutoDiscovery.ImplicitlyChangingWorkItemGroupDefinition.Suffix}";

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItemGroups");
        responseDocument.Data.SingleValue.Id.Should().Be(existingGroup.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("name").With(value => value.Should().Be(groupName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("isPublic").With(value => value.Should().Be(existingGroup.IsPublic));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItemGroup groupInDatabase = await dbContext.Groups.FirstWithIdAsync(existingGroup.Id);

            groupInDatabase.Name.Should().Be(groupName);
            groupInDatabase.IsPublic.Should().Be(existingGroup.IsPublic);
        });
    }

    [Fact]
    public async Task Can_completely_update_resource_with_string_ID()
    {
        // Arrange
        RgbColor existingColor = _fakers.RgbColor.Generate();
        string newDisplayName = _fakers.RgbColor.Generate().DisplayName;

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

        string route = $"/rgbColors/{existingColor.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.FirstWithIdAsync(existingColor.Id);

            colorInDatabase.DisplayName.Should().Be(newDisplayName);
        });
    }

    [Fact]
    public async Task Can_update_resource_without_side_effects()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();
        UserAccount newUserAccount = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(existingUserAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = existingUserAccount.StringId,
                attributes = new
                {
                    firstName = newUserAccount.FirstName,
                    lastName = newUserAccount.LastName
                }
            }
        };

        string route = $"/userAccounts/{existingUserAccount.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            UserAccount userAccountInDatabase = await dbContext.UserAccounts.FirstWithIdAsync(existingUserAccount.Id);

            userAccountInDatabase.FirstName.Should().Be(newUserAccount.FirstName);
            userAccountInDatabase.LastName.Should().Be(newUserAccount.LastName);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_side_effects()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        string newDescription = _fakers.WorkItem.Generate().Description!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                id = existingWorkItem.StringId,
                attributes = new
                {
                    description = newDescription,
                    dueAt = (DateTime?)null
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        string itemDescription = $"{newDescription}{ContainerTypeToHideFromAutoDiscovery.ImplicitlyChangingWorkItemDefinition.Suffix}";

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Id.Should().Be(existingWorkItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(itemDescription));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("dueAt").With(value => value.Should().BeNull());
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(existingWorkItem.Priority));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("isImportant").With(value => value.Should().Be(existingWorkItem.IsImportant));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Description.Should().Be(itemDescription);
            workItemInDatabase.DueAt.Should().BeNull();
            workItemInDatabase.Priority.Should().Be(existingWorkItem.Priority);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_side_effects_with_primary_fieldset()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        string newDescription = _fakers.WorkItem.Generate().Description!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                id = existingWorkItem.StringId,
                attributes = new
                {
                    description = newDescription,
                    dueAt = (DateTime?)null
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}?fields[workItems]=description,priority";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        string itemDescription = $"{newDescription}{ContainerTypeToHideFromAutoDiscovery.ImplicitlyChangingWorkItemDefinition.Suffix}";

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Id.Should().Be(existingWorkItem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(2);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(itemDescription));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("priority").With(value => value.Should().Be(existingWorkItem.Priority));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.FirstWithIdAsync(existingWorkItem.Id);

            workItemInDatabase.Description.Should().Be(itemDescription);
            workItemInDatabase.DueAt.Should().BeNull();
            workItemInDatabase.Priority.Should().Be(existingWorkItem.Priority);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_on_unknown_resource_ID_in_url()
    {
        // Arrange
        string workItemId = Unknown.StringId.For<WorkItem, string?>();

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                id = workItemId
            }
        };

        string route = $"/workItems/{workItemId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'workItems' with ID '{workItemId}' does not exist.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }
}
