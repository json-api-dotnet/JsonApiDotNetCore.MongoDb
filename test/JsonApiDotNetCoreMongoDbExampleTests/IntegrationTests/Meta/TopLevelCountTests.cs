using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Meta
{
    public sealed class TopLevelCountTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly SupportFakers _fakers = new SupportFakers();

        public TopLevelCountTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Renders_resource_count_for_collection()
        {
            // Arrange
            SupportTicket ticket = _fakers.SupportTicket.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<SupportTicket>();
                await db.GetCollection<SupportTicket>().InsertOneAsync(ticket);
            });

            const string route = "/supportTickets";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().NotBeNull();
            responseDocument.Meta["totalResources"].Should().Be(1);
        }

        [Fact]
        public async Task Renders_resource_count_for_empty_collection()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<SupportTicket>();
            });

            const string route = "/supportTickets";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().NotBeNull();
            responseDocument.Meta["totalResources"].Should().Be(0);
        }

        [Fact]
        public async Task Hides_resource_count_in_create_resource_response()
        {
            // Arrange
            string newDescription = _fakers.SupportTicket.Generate().Description;

            var requestBody = new
            {
                data = new
                {
                    type = "supportTickets",
                    attributes = new
                    {
                        description = newDescription
                    }
                }
            };

            const string route = "/supportTickets";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Meta.Should().BeNull();
        }

        [Fact]
        public async Task Hides_resource_count_in_update_resource_response()
        {
            // Arrange
            SupportTicket existingTicket = _fakers.SupportTicket.Generate();

            string newDescription = _fakers.SupportTicket.Generate().Description;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<SupportTicket>().InsertOneAsync(existingTicket);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "supportTickets",
                    id = existingTicket.StringId,
                    attributes = new
                    {
                        description = newDescription
                    }
                }
            };

            string route = "/supportTickets/" + existingTicket.StringId;

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.Should().BeNull();
        }
    }
}
