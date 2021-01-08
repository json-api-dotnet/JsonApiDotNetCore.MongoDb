using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Filtering
{
    public sealed class FilterTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;

        public FilterTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;
        }

        [Fact]
        public async Task Can_filter_on_ID()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "Jane"
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<Person>();
                await db.GetCollection<Person>().InsertManyAsync(new[] {person, new Person()});
            });

            var route = $"/api/v1/people?filter=equals(id,'{person.StringId}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(person.StringId);
            responseDocument.ManyData[0].Attributes["firstName"].Should().Be(person.FirstName);
        }
    }
}
