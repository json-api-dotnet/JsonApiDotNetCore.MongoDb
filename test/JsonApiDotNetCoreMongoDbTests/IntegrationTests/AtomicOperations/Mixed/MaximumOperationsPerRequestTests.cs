using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Mixed;

[Collection("AtomicOperationsFixture")]
public sealed class MaximumOperationsPerRequestTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;

    public MaximumOperationsPerRequestTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;
    }

    [Fact]
    public async Task Can_process_high_number_of_operations_when_unconstrained()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.MaximumOperationsPerRequest = null;

        const int elementCount = 100;

        var operationElements = new List<object>(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            operationElements.Add(new
            {
                op = "add",
                data = new
                {
                    type = "performers",
                    attributes = new
                    {
                    }
                }
            });
        }

        var requestBody = new
        {
            atomic__operations = operationElements
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
