using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Updating.Resources;

public sealed class UpdateToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public UpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(WorkItem).Namespace);

        testContext.UseController<WorkItemGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_ToOne_relationship()
    {
        // Arrange
        WorkItemGroup existingGroup = _fakers.WorkItemGroup.Generate();
        RgbColor existingColor = _fakers.RgbColor.Generate();

        await _testContext.RunOnDatabaseAsync(dbContext =>
        {
            dbContext.RgbColors.Add(existingColor);
            dbContext.Groups.Add(existingGroup);
            return dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = existingGroup.StringId,
                relationships = new
                {
                    color = new
                    {
                        data = new
                        {
                            type = "rgbColors",
                            id = existingColor.StringId
                        }
                    }
                }
            }
        };

        string route = $"/workItemGroups/{existingGroup.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }
}
