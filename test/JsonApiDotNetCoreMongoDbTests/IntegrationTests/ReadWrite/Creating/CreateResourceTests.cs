using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Creating;

public sealed class CreateResourceTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public CreateResourceTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(WorkItem).Namespace);

        testContext.UseController<WorkItemsController>();
        testContext.UseController<RgbColorsController>();
        testContext.UseController<ModelWithIntIdsController>();
    }

    [Fact]
    public async Task Can_create_resource_with_string_ID()
    {
        // Arrange
        WorkItem newWorkItem = _fakers.WorkItem.GenerateOne();
        newWorkItem.DueAt = null;

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    description = newWorkItem.Description
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("description").WhoseValue.Should().Be(newWorkItem.Description);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("dueAt").WhoseValue.Should().Be(newWorkItem.DueAt);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        string newWorkItemId = responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Description.Should().Be(newWorkItem.Description);
            workItemInDatabase.DueAt.Should().Be(newWorkItem.DueAt);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_int_ID()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "modelWithIntIds",
                attributes = new
                {
                    description = "Test"
                }
            }
        };

        const string route = "/modelWithIntIds";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.Title.Should().Be("An unhandled error occurred while processing this request.");
        error.Detail.Should().Be("MongoDB can only be used with resources that implement 'IMongoIdentifiable'.");
    }

    [Fact]
    public async Task Can_create_resource_without_attributes_or_relationships()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                },
                relationship = new
                {
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("description").WhoseValue.Should().BeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("dueAt").WhoseValue.Should().BeNull();
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        string newWorkItemId = responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Description.Should().BeNull();
            workItemInDatabase.DueAt.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_client_generated_ID()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = "0A0B0C",
                attributes = new
                {
                    displayName = "Black"
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: The use of client-generated IDs is disabled.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/id");
        error.Meta.Should().HaveRequestBody();
    }
}
