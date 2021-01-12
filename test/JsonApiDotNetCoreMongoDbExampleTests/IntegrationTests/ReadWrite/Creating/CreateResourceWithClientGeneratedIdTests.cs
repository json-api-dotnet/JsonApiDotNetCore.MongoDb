using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceWithClientGeneratedIdTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public CreateResourceWithClientGeneratedIdTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }
        
        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
        {
            // Arrange
            var newColor = _fakers.RgbColor.Generate();
            newColor.Id = "507f191e810c19729de860ea";
            
            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = newColor.StringId,
                    attributes = new
                    {
                        displayName = newColor.DisplayName
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var colorInDatabase = await db.GetCollection<RgbColor>().AsQueryable()
                    .Where(color => color.Id == newColor.Id)
                    .FirstOrDefaultAsync();

                colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
            });

            var property = typeof(RgbColor).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_side_effects_with_fieldset()
        {
            // Arrange
            var newGroup = _fakers.WorkItemGroup.Generate();
            newGroup.Id = "5ffcc0d1d69a27c92b8c62dd";

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    id = newGroup.StringId,
                    attributes = new
                    {
                        name = newGroup.Name
                    }
                }
            };

            var route = "/workItemGroups?fields[workItemGroups]=name";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItemGroups");
            responseDocument.SingleData.Id.Should().Be(newGroup.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["name"].Should().Be(newGroup.Name);
            responseDocument.SingleData.Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var groupInDatabase = await db.GetCollection<WorkItemGroup>().AsQueryable()
                    .Where(group => group.Id == newGroup.Id)
                    .FirstOrDefaultAsync();

                groupInDatabase.Name.Should().Be(newGroup.Name);
            });

            var property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Cannot_create_resource_for_existing_client_generated_ID()
        {
            // Arrange
            var existingColor = _fakers.RgbColor.Generate();

            var colorToCreate = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<RgbColor>().InsertOneAsync(existingColor);
                colorToCreate.Id = existingColor.Id;
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = colorToCreate.StringId,
                    attributes = new
                    {
                        displayName = colorToCreate.DisplayName
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Another resource with the specified ID already exists.");
            responseDocument.Errors[0].Detail.Should().Be($"Another resource of type 'rgbColors' with ID '{existingColor.StringId}' already exists.");
        }
    }
}