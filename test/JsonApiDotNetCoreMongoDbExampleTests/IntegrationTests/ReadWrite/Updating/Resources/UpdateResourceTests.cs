using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite.Updating.Resources
{
    public sealed class UpdateResourceTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public UpdateResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_update_resource_without_attributes_or_relationships()
        {
            // Arrange
            UserAccount existingUserAccount = _fakers.UserAccount.Generate();

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

            string route = "/userAccounts/" + existingUserAccount.StringId;

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_update_resource_with_unknown_attribute()
        {
            // Arrange
            UserAccount existingUserAccount = _fakers.UserAccount.Generate();
            string newFirstName = _fakers.UserAccount.Generate().FirstName;

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

            string route = "/userAccounts/" + existingUserAccount.StringId;

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                UserAccount userAccountInDatabase = await db.GetCollection<UserAccount>().AsQueryable().FirstWithIdAsync(existingUserAccount.Id);

                userAccountInDatabase.FirstName.Should().Be(newFirstName);
                userAccountInDatabase.LastName.Should().Be(existingUserAccount.LastName);
            });
        }

        [Fact]
        public async Task Can_partially_update_resource_with_string_ID()
        {
            // Arrange
            WorkItemGroup existingGroup = _fakers.WorkItemGroup.Generate();
            string newName = _fakers.WorkItemGroup.Generate().Name;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItemGroup>().InsertOneAsync(existingGroup);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    id = existingGroup.StringId,
                    attributes = new
                    {
                        name = newName
                    }
                }
            };

            string route = "/workItemGroups/" + existingGroup.StringId;

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItemGroups");
            responseDocument.SingleData.Id.Should().Be(existingGroup.StringId);
            responseDocument.SingleData.Attributes["name"].Should().Be(newName);
            responseDocument.SingleData.Attributes["isPublic"].Should().Be(existingGroup.IsPublic);
            responseDocument.SingleData.Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                WorkItemGroup groupInDatabase = await db.GetCollection<WorkItemGroup>().AsQueryable().FirstWithIdAsync(existingGroup.Id);

                groupInDatabase.Name.Should().Be(newName);
                groupInDatabase.IsPublic.Should().Be(existingGroup.IsPublic);
            });

            PropertyInfo property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Can_completely_update_resource_with_string_ID()
        {
            // Arrange
            RgbColor existingColor = _fakers.RgbColor.Generate();
            string newDisplayName = _fakers.RgbColor.Generate().DisplayName;

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

            string route = "/rgbColors/" + existingColor.StringId;

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                RgbColor colorInDatabase = await db.GetCollection<RgbColor>().AsQueryable().FirstWithIdAsync(existingColor.Id);

                colorInDatabase.DisplayName.Should().Be(newDisplayName);
            });

            PropertyInfo property = typeof(RgbColor).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Can_update_resource_without_side_effects()
        {
            // Arrange
            UserAccount existingUserAccount = _fakers.UserAccount.Generate();
            UserAccount newUserAccount = _fakers.UserAccount.Generate();

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

            string route = "/userAccounts/" + existingUserAccount.StringId;

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                UserAccount userAccountInDatabase = await db.GetCollection<UserAccount>().AsQueryable().FirstWithIdAsync(existingUserAccount.Id);

                userAccountInDatabase.FirstName.Should().Be(newUserAccount.FirstName);
                userAccountInDatabase.LastName.Should().Be(newUserAccount.LastName);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_side_effects()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            string newDescription = _fakers.WorkItem.Generate().Description;

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

            string route = "/workItems/" + existingWorkItem.StringId;

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Id.Should().Be(existingWorkItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(newDescription);
            responseDocument.SingleData.Attributes["dueAt"].Should().BeNull();
            responseDocument.SingleData.Attributes["priority"].Should().Be(existingWorkItem.Priority.ToString("G"));
            responseDocument.SingleData.Attributes.Should().ContainKey("concurrencyToken");
            responseDocument.SingleData.Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                WorkItem workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable().FirstWithIdAsync(existingWorkItem.Id);

                workItemInDatabase.Description.Should().Be(newDescription);
                workItemInDatabase.DueAt.Should().BeNull();
                workItemInDatabase.Priority.Should().Be(existingWorkItem.Priority);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_side_effects_with_primary_fieldset()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            string newDescription = _fakers.WorkItem.Generate().Description;

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

            string route = $"/workItems/{existingWorkItem.StringId}?fields[workItems]=description,priority";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

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
                WorkItem workItemInDatabase = await db.GetCollection<WorkItem>().AsQueryable().FirstWithIdAsync(existingWorkItem.Id);

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
                    id = "ffffffffffffffffffffffff"
                }
            };

            const string route = "/workItems/ffffffffffffffffffffffff";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be("Resource of type 'workItems' with ID 'ffffffffffffffffffffffff' does not exist.");
        }
    }
}
