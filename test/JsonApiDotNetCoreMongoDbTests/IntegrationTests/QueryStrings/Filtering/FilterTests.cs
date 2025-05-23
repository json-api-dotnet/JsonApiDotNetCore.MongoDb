using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterTests : IClassFixture<IntegrationTestContext<TestableStartup, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public FilterTests(IntegrationTestContext<TestableStartup, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(Blog).Namespace);

        testContext.UseController<WebAccountsController>();
    }

    [Fact]
    public async Task Can_filter_on_ID()
    {
        // Arrange
        List<WebAccount> accounts = _fakers.WebAccount.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WebAccount>();
            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts?filter=equals(id,'{accounts[0].StringId}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(accounts[0].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("userName").WhoseValue.Should().Be(accounts[0].UserName);
    }
}
