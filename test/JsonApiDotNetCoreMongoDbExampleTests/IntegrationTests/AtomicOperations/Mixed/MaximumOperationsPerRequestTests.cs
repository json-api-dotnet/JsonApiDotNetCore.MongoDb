using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations.Mixed
{
    [Collection("AtomicOperationsFixture")]
    public sealed class MaximumOperationsPerRequestTests
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;

        public MaximumOperationsPerRequestTests(AtomicOperationsFixture fixture)
        {
            _testContext = fixture.TestContext;

            fixture.TestContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_process_high_number_of_operations_when_unconstrained()
        {
            // Arrange
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumOperationsPerRequest = null;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.EnsureEmptyCollectionAsync<Performer>();
            });

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
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }
    }
}
