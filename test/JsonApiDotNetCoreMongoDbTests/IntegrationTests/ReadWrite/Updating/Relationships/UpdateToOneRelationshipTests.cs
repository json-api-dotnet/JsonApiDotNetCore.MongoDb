using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Updating.Relationships;

public sealed class UpdateToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public UpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemGroupsController>();
    }

    [Fact]
    public async Task Cannot_replace_ToOne_relationship()
    {
        // Arrange
        WorkItemGroup existingGroup = _fakers.WorkItemGroup.Generate();
        RgbColor existingColor = _fakers.RgbColor.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.RgbColors.Add(existingColor);
            dbContext.Groups.Add(existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = existingColor.StringId
            }
        };

        string route = $"/workItemGroups/{existingGroup.StringId}/relationships/color";

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
