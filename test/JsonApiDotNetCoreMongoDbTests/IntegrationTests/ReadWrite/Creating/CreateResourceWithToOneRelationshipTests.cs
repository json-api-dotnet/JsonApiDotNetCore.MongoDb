using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Creating;

public sealed class CreateResourceWithToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public CreateResourceWithToOneRelationshipTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_with_ToOne_relationship()
    {
        // Arrange
        WorkItemGroup existingGroup = _fakers.WorkItemGroup.Generate();
        existingGroup.Color = _fakers.RgbColor.Generate();

        string newGroupName = _fakers.WorkItemGroup.Generate().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.RgbColors.Add(existingGroup.Color);
            dbContext.Groups.Add(existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                attributes = new
                {
                    name = newGroupName
                },
                relationships = new
                {
                    color = new
                    {
                        data = new
                        {
                            type = "rgbColors",
                            id = existingGroup.Color.StringId
                        }
                    }
                }
            }
        };

        const string route = "/workItemGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

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
