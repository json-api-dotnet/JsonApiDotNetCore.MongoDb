using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Updating.Relationships;

public sealed class ReplaceToManyRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public ReplaceToManyRelationshipTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
    }

    [Fact]
    public async Task Cannot_replace_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        UserAccount existingSubscriber = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(existingSubscriber);
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "userAccounts",
                    id = existingSubscriber.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_replace_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        WorkTag existingTag = _fakers.WorkTag.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkTags.Add(existingTag);
            dbContext.WorkItems.Add(existingWorkItem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workTags",
                    id = existingTag.StringId
                }
            }
        };

        string route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }
}
