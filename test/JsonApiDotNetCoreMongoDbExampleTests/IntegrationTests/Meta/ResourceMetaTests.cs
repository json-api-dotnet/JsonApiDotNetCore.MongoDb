using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Meta
{
    public sealed class ResourceMetaTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly SupportFakers _fakers = new SupportFakers();

        public ResourceMetaTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Returns_resource_meta_from_ResourceDefinition()
        {
            // Arrange
            List<SupportTicket> tickets = _fakers.SupportTicket.Generate(3);
            tickets[0].Description = "Critical: " + tickets[0].Description;
            tickets[2].Description = "Critical: " + tickets[2].Description;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<SupportTicket>();
                await db.GetCollection<SupportTicket>().InsertManyAsync(tickets);
            });

            const string route = "/supportTickets";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Meta.Should().ContainKey("hasHighPriority");
            responseDocument.ManyData[1].Meta.Should().BeNull();
            responseDocument.ManyData[2].Meta.Should().ContainKey("hasHighPriority");
        }
    }
}
