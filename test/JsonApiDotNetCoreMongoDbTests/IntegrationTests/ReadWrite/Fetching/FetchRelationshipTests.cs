using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ReadWrite.Fetching;

public sealed class FetchRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public FetchRelationshipTests(IntegrationTestContext<TestableStartup, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<UserAccountsController>();
    }

    [Fact]
    public async Task Cannot_get_ManyToOne_relationship()
    {
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

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
    public async Task Cannot_get_OneToMany_relationship()
    {
        // Arrange
        UserAccount userAccount = _fakers.UserAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.Add(userAccount);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/userAccounts/{userAccount.StringId}/relationships/assignedItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

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
    public async Task Cannot_get_ManyToMany_relationship()
    {
        // Arrange
        WorkItem workItem = _fakers.WorkItem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}/relationships/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

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
