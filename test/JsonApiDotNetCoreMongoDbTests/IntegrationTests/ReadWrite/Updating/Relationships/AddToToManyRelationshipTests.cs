using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Updating.Relationships;

public sealed class AddToToManyRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public AddToToManyRelationshipTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(WorkItem).Namespace);

        testContext.UseController<WorkItemsController>();
    }

    [Fact]
    public async Task Cannot_add_to_OneToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        UserAccount existingSubscriber = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(dbContext =>
        {
            dbContext.UserAccounts.Add(existingSubscriber);
            dbContext.WorkItems.Add(existingWorkItem);
            return dbContext.SaveChangesAsync();
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

    [Fact]
    public async Task Cannot_add_to_ManyToMany_relationship()
    {
        // Arrange
        WorkItem existingWorkItem = _fakers.WorkItem.Generate();
        WorkTag existingTag = _fakers.WorkTag.Generate();

        await _testContext.RunOnDatabaseAsync(dbContext =>
        {
            dbContext.WorkTags.Add(existingTag);
            dbContext.WorkItems.Add(existingWorkItem);
            return dbContext.SaveChangesAsync();
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
