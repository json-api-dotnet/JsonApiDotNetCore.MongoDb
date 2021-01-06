using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<RgbColor, string>();
                builder.Add<UserAccount, string>();
                builder.Add<WorkItem, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoDbRepository<RgbColor>>();
                services.AddResourceRepository<MongoDbRepository<UserAccount>>();
                services.AddResourceRepository<MongoDbRepository<WorkItem>>();
            });

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
                await db.GetCollection<UserAccount>(nameof(UserAccount)).InsertOneAsync(existingUserAccount);
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
                await db.GetCollection<UserAccount>(nameof(UserAccount)).InsertOneAsync(existingUserAccount);
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
                var userAccountInDatabase = await (await db.GetCollection<UserAccount>(nameof(UserAccount))
                    .FindAsync(Builders<UserAccount>.Filter.Eq(userAccount => userAccount.Id, existingUserAccount.Id)))
                    .FirstAsync();

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
                await db.GetCollection<RgbColor>(nameof(RgbColor)).InsertOneAsync(existingColor);
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
                var colorInDatabase = await (await db.GetCollection<RgbColor>(nameof(RgbColor))
                    .FindAsync(Builders<RgbColor>.Filter.Eq(color => color.Id, existingColor.Id)))
                    .FirstAsync();

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
                await db.GetCollection<UserAccount>(nameof(UserAccount)).InsertOneAsync(existingUserAccount);
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
                var userAccountInDatabase = await (await db.GetCollection<UserAccount>(nameof(UserAccount))
                    .FindAsync(Builders<UserAccount>.Filter.Eq(userAccount => userAccount.Id, existingUserAccount.Id)))
                    .FirstAsync();

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
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
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
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                    .FindAsync(Builders<WorkItem>.Filter.Eq(workItem => workItem.Id, existingWorkItem.Id)))
                    .FirstAsync();

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
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
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
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                    .FindAsync(Builders<WorkItem>.Filter.Eq(workItem => workItem.Id, existingWorkItem.Id)))
                    .FirstAsync();

                workItemInDatabase.Description.Should().Be(newDescription);
                workItemInDatabase.DueAt.Should().BeNull();
                workItemInDatabase.Priority.Should().Be(existingWorkItem.Priority);
            });
        }

        [Fact]
        public async Task Cannot_update_resource_for_missing_request_body()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = string.Empty;

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Missing request body.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_update_resource_for_missing_type()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    id = existingWorkItem.StringId
                }
            };

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in 'data' element. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_update_resource_for_unknown_type()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "doesNotExist",
                    id = existingWorkItem.StringId
                }
            };

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource type 'doesNotExist' does not exist. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_update_resource_for_missing_ID()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems"
                }
            };

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'id' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Request body: <<");
        }

        [Fact]
        public async Task Cannot_update_resource_on_unknown_resource_type_in_url()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId
                }
            };

            var route = "/doesNotExist/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
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
        public async Task Cannot_update_on_resource_type_mismatch_between_url_and_body()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = existingWorkItem.StringId
                }
            };

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Resource type mismatch between request body and endpoint URL.");
            responseDocument.Errors[0].Detail.Should().Be($"Expected resource of type 'workItems' in PATCH request body at endpoint '/workItems/{existingWorkItem.StringId}', instead of 'rgbColors'.");
        }

        [Fact]
        public async Task Cannot_update_on_resource_ID_mismatch_between_url_and_body()
        {
            // Arrange
            var existingWorkItems = _fakers.WorkItem.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertManyAsync(existingWorkItems);
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
        public async Task Cannot_update_resource_attribute_with_blocked_capability()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    attributes = new
                    {
                        concurrencyToken = "274E1D9A-91BE-4A42-B648-CA75E8B2945E"
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
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Changing the value of the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().StartWith("Changing the value of 'concurrencyToken' is not allowed. - Request body:");
        }

        [Fact]
        public async Task Cannot_update_resource_for_broken_JSON_request_body()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = "{ \"data\" {";

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Invalid character after parsing");
        }

        [Fact]
        public async Task Cannot_change_ID_of_existing_resource()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    attributes = new
                    {
                        id = existingWorkItem.Id + 123456
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
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Resource ID is read-only.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource ID is read-only. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_update_resource_with_incompatible_attribute_value()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<WorkItem>(nameof(WorkItem)).InsertOneAsync(existingWorkItem);
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
