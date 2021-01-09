using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Updating.Resources
{
    public sealed class UpdateResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public UpdateResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;
            options.AllowClientGeneratedIds = false;
        }

        [Fact]
        public async Task Can_update_resource_without_attributes_or_relationships()
        {
            // Arrange
            var existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(existingUserAccount);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = existingUserAccount.StringId,
                    attributes = new
                    {
                    },
                    relationships = new
                    {
                    }
                }
            };

            var route = "/userAccounts/" + existingUserAccount.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_update_resource_with_unknown_attribute()
        {
            // Arrange
            var existingUserAccount = _fakers.UserAccount.Generate();
            var newFirstName = _fakers.UserAccount.Generate().FirstName;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(existingUserAccount);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = existingUserAccount.StringId,
                    attributes = new
                    {
                        firstName = newFirstName,
                        doesNotExist = "Ignored"
                    }
                }
            };

            var route = "/userAccounts/" + existingUserAccount.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var userAccountInDatabase = await db.GetCollection<UserAccount>().AsQueryable()
                    .Where(userAccount => userAccount.Id == existingUserAccount.Id)
                    .FirstOrDefaultAsync();

                userAccountInDatabase.FirstName.Should().Be(newFirstName);
                userAccountInDatabase.LastName.Should().Be(existingUserAccount.LastName);
            });
        }

        [Fact]
        public async Task Can_completely_update_resource_with_string_ID()
        {
            // Arrange
            var existingColor = _fakers.RgbColor.Generate();
            var newDisplayName = _fakers.RgbColor.Generate().DisplayName;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<RgbColor>().InsertOneAsync(existingColor);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = existingColor.StringId,
                    attributes = new
                    {
                        displayName = newDisplayName
                    }
                }
            };

            var route = "/rgbColors/" + existingColor.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var colorInDatabase = await db.GetCollection<RgbColor>().AsQueryable()
                    .Where(color => color.Id == existingColor.Id)
                    .FirstOrDefaultAsync();

                colorInDatabase.DisplayName.Should().Be(newDisplayName);
            });

            var property = typeof(RgbColor).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Can_update_resource_without_side_effects()
        {
            // Arrange
            var existingUserAccount = _fakers.UserAccount.Generate();
            var newUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<UserAccount>().InsertOneAsync(existingUserAccount);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = existingUserAccount.StringId,
                    attributes = new
                    {
                        firstName = newUserAccount.FirstName,
                        lastName = newUserAccount.LastName
                    }
                }
            };

            var route = "/userAccounts/" + existingUserAccount.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var userAccountInDatabase = await db.GetCollection<UserAccount>().AsQueryable()
                    .Where(workItem => workItem.Id == existingUserAccount.Id)
                    .FirstOrDefaultAsync();

                userAccountInDatabase.FirstName.Should().Be(newUserAccount.FirstName);
                userAccountInDatabase.LastName.Should().Be(newUserAccount.LastName);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_side_effects()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            var newDescription = _fakers.WorkItem.Generate().Description;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    attributes = new
                    {
                        description = newDescription,
                        dueAt = (DateTime?)null
                    }
                }
            };

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Id.Should().Be(existingWorkItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(newDescription);
            responseDocument.SingleData.Attributes["dueAt"].Should().BeNull();
            responseDocument.SingleData.Attributes["priority"].Should().Be(existingWorkItem.Priority.ToString("G"));
            responseDocument.SingleData.Attributes.Should().ContainKey("concurrencyToken");

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable()
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstOrDefaultAsync();

                workItemInDatabase.Description.Should().Be(newDescription);
                workItemInDatabase.DueAt.Should().BeNull();
                workItemInDatabase.Priority.Should().Be(existingWorkItem.Priority);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_side_effects_with_primary_fieldset()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            var newDescription = _fakers.WorkItem.Generate().Description;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    attributes = new
                    {
                        description = newDescription,
                        dueAt = (DateTime?)null
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}?fields[workItems]=description,priority";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Id.Should().Be(existingWorkItem.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(2);
            responseDocument.SingleData.Attributes["description"].Should().Be(newDescription);
            responseDocument.SingleData.Attributes["priority"].Should().Be(existingWorkItem.Priority.ToString("G"));
            responseDocument.SingleData.Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable()
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstOrDefaultAsync();

                workItemInDatabase.Description.Should().Be(newDescription);
                workItemInDatabase.DueAt.Should().BeNull();
                workItemInDatabase.Priority.Should().Be(existingWorkItem.Priority);
            });
        }

        [Fact]
        public async Task Cannot_update_resource_on_unknown_resource_ID_in_url()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = "5f88857c4aa60defec6a4999"
                }
            };

            var route = "/workItems/5f88857c4aa60defec6a4999";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID '5f88857c4aa60defec6a4999' does not exist.");
        }

        [Fact]
        public async Task Cannot_update_on_resource_ID_mismatch_between_url_and_body()
        {
            // Arrange
            var existingWorkItems = _fakers.WorkItem.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertManyAsync(existingWorkItems);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItems[0].StringId
                }
            };

            var route = "/workItems/" + existingWorkItems[1].StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Resource ID mismatch between request body and endpoint URL.");
            responseDocument.Errors[0].Detail.Should().Be($"Expected resource ID '{existingWorkItems[1].StringId}' in PATCH request body at endpoint '/workItems/{existingWorkItems[1].StringId}', instead of '{existingWorkItems[0].StringId}'.");
        }

        [Fact]
        public async Task Cannot_update_resource_with_incompatible_attribute_value()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>().InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    attributes = new
                    {
                        dueAt = "not-a-valid-time"
                    }
                }
            };

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Failed to convert 'not-a-valid-time' of type 'String' to type 'Nullable`1'. - Request body: <<");
        }
    }
}
