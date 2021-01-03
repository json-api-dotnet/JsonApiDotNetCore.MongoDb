using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite.Updating.Resources
{
    public sealed class UpdateRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public UpdateRelationshipTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<RgbColor, string>();
                builder.Add<WorkItemGroup, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoDbRepository<RgbColor>>();
                services.AddResourceRepository<MongoDbRepository<WorkItemGroup>>();
            });

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;
            options.AllowClientGeneratedIds = false;
        }

        [Fact]
        public async Task Cannot_create_OneToOne_relationship_from_principal_side()
        {
            var existingGroup = _fakers.WorkItemGroup.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
                await db.GetCollection<WorkItemGroup>(nameof(WorkItemGroup)).InsertOneAsync(existingGroup));
            
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    id = existingGroup.StringId,
                    relationships = new
                    {
                        color = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = ObjectId.GenerateNewId().ToString()
                            }
                        }
                    }
                }
            };

            var route = "/workItemGroups/" + existingGroup.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }
    }
}