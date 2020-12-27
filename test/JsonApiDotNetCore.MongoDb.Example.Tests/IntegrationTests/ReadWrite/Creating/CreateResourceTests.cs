using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public CreateResourceTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<RgbColor, string>();
                builder.Add<WorkItem, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoEntityRepository<RgbColor, string>>();
                services.AddResourceRepository<MongoEntityRepository<WorkItem, string>>();
            });

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;
            options.AllowClientGeneratedIds = false;
        }

        [Fact]
        public async Task Sets_location_header_for_created_resource()
        {
            // Arrange
            var newWorkItem = _fakers.WorkItem.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = newWorkItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            var newWorkItemId = responseDocument.SingleData.Id;
            httpResponse.Headers.Location.Should().Be("/workItems/" + newWorkItemId);

            responseDocument.Links.Self.Should().Be("http://localhost/workItems");
            responseDocument.Links.First.Should().BeNull();

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Self.Should().Be("http://localhost" + httpResponse.Headers.Location);
        }

        [Fact]
        public async Task Can_create_resource_with_ID()
        {
            // Arrange
            var newWorkItem = _fakers.WorkItem.Generate();
            newWorkItem.DueAt = null;

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = newWorkItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(newWorkItem.Description);
            responseDocument.SingleData.Attributes["dueAt"].Should().Be(newWorkItem.DueAt);
            // responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                        .FindAsync(Builders<WorkItem>.Filter.Eq(w => w.Id, newWorkItemId)))
                        .FirstAsync();

                workItemInDatabase.Description.Should().Be(newWorkItem.Description);
                workItemInDatabase.DueAt.Should().Be(newWorkItem.DueAt);
            });

            var property = typeof(WorkItem).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        // [Fact]
        // public async Task Can_create_resource_without_attributes_or_relationships()
        // {
        //     // Arrange
        //     var requestBody = new
        //     {
        //         data = new
        //         {
        //             type = "workItems",
        //             attributes = new
        //             {
        //             },
        //             relationship = new
        //             {
        //             }
        //         }
        //     };
        //
        //     var route = "/workItems";
        //
        //     // Act
        //     var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);
        //
        //     // Assert
        //     httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);
        //
        //     responseDocument.SingleData.Should().NotBeNull();
        //     responseDocument.SingleData.Type.Should().Be("workItems");
        //     responseDocument.SingleData.Attributes["description"].Should().BeNull();
        //     responseDocument.SingleData.Attributes["dueAt"].Should().BeNull();
        //     responseDocument.SingleData.Relationships.Should().NotBeEmpty();
        //
        //     var newWorkItemId = int.Parse(responseDocument.SingleData.Id);
        //
        //     await _testContext.RunOnDatabaseAsync(async dbContext =>
        //     {
        //         var workItemInDatabase = await dbContext.WorkItems
        //             .FirstAsync(workItem => workItem.Id == newWorkItemId);
        //
        //         workItemInDatabase.Description.Should().BeNull();
        //         workItemInDatabase.DueAt.Should().BeNull();
        //     });
        // }

        [Fact]
        public async Task Can_create_resource_with_unknown_attribute()
        {
            // Arrange
            var newWorkItem = _fakers.WorkItem.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        doesNotExist = "ignored",
                        description = newWorkItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(newWorkItem.Description);
            // responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                    .FindAsync(Builders<WorkItem>.Filter.Eq(workItem => workItem.Id, newWorkItemId)))
                    .FirstAsync();

                workItemInDatabase.Description.Should().Be(newWorkItem.Description);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_unknown_relationship()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        doesNotExist = new
                        {
                            data = new
                            {
                                type = "doesNotExist",
                                id = 12345678
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes.Should().NotBeEmpty();
            // responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var workItemInDatabase = await (await db.GetCollection<WorkItem>(nameof(WorkItem))
                    .FindAsync(Builders<WorkItem>.Filter.Eq(workItem => workItem.Id, newWorkItemId)))
                    .FirstOrDefaultAsync();

                workItemInDatabase.Should().NotBeNull();
            });
        }

        [Fact]
        public async Task Cannot_create_resource_with_client_generated_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = "0A0B0C",
                    attributes = new
                    {
                        name = "Black"
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("Specifying the resource ID in POST requests is not allowed.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/id");
        }

        [Fact]
        public async Task Cannot_create_resource_for_missing_request_body()
        {
            // Arrange
            var requestBody = string.Empty;

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Missing request body.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_create_resource_for_missing_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    attributes = new
                    {
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in 'data' element. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_for_unknown_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "doesNotExist",
                    attributes = new
                    {
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource type 'doesNotExist' does not exist. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_on_unknown_resource_type_in_url()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                    }
                }
            };

            var route = "/doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_create_on_resource_type_mismatch_between_url_and_body()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = "0A0B0C"
                }
            };
            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Resource type mismatch between request body and endpoint URL.");
            responseDocument.Errors[0].Detail.Should().Be("Expected resource of type 'workItems' in POST request body at endpoint '/workItems', instead of 'rgbColors'.");
        }

        [Fact]
        public async Task Cannot_create_resource_attribute_with_blocked_capability()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        concurrencyToken = "274E1D9A-91BE-4A42-B648-CA75E8B2945E"
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Setting the initial value of the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().StartWith("Setting the initial value of 'concurrencyToken' is not allowed. - Request body:");
        }

        // [Fact]
        // public async Task Cannot_create_resource_with_readonly_attribute()
        // {
        //     // Arrange
        //     var requestBody = new
        //     {
        //         data = new
        //         {
        //             type = "workItemGroups",
        //             attributes = new
        //             {
        //                 concurrencyToken = "274E1D9A-91BE-4A42-B648-CA75E8B2945E"
        //             }
        //         }
        //     };
        //
        //     var route = "/workItemGroups";
        //
        //     // Act
        //     var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);
        //
        //     // Assert
        //     httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);
        //
        //     responseDocument.Errors.Should().HaveCount(1);
        //     responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        //     responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Attribute is read-only.");
        //     responseDocument.Errors[0].Detail.Should().StartWith("Attribute 'concurrencyToken' is read-only. - Request body:");
        // }

        [Fact]
        public async Task Cannot_create_resource_for_broken_JSON_request_body()
        {
            // Arrange
            var requestBody = "{ \"data\" {";

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Invalid character after parsing");
        }

        [Fact]
        public async Task Cannot_create_resource_with_incompatible_attribute_value()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        dueAt = "not-a-valid-time"
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Failed to convert 'not-a-valid-time' of type 'String' to type 'Nullable`1'. - Request body: <<");
        }

        //[Fact]
        //public async Task Can_create_resource_with_attributes_and_multiple_relationship_types()
        //{
            //// Arrange
            //var existingUserAccounts = _fakers.UserAccount.Generate(2);
            //var existingTag = _fakers.WorkTag.Generate();

            //var newDescription = _fakers.WorkItem.Generate().Description;

            //await _testContext.RunOnDatabaseAsync(async db =>
            //{
                //await db.GetCollection<UserAccount>(nameof(UserAccount)).InsertManyAsync(existingUserAccounts);
                //await db.GetCollection<WorkTag>(nameof(WorkTag)).InsertOneAsync(existingTag);
            //});

            //var requestBody = new
            //{
                //data = new
                //{
                    //type = "workItems",
                    //attributes = new
                    //{
                        //description = newDescription
                    //},
                    //relationships = new
                    //{
                        //assignee = new
                        //{
                            //data = new
                            //{
                                //type = "userAccounts",
                                //id = existingUserAccounts[0].StringId
                            //}
                        //},
                        //subscribers = new
                        //{
                            //data = new[]
                            //{
                                //new
                                //{
                                    //type = "userAccounts",
                                    //id = existingUserAccounts[1].StringId
                                //}
                            //}
                        //},
                        //tags = new
                        //{
                            //data = new[]
                            //{
                                //new
                                //{
                                    //type = "workTags",
                                    //id = existingTag.StringId
                                //}
                            //}
                        //}
                    //}
                //}
            //};

            //var route = "/workItems";

            //// Act
            //var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            //// Assert
            //httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            //responseDocument.SingleData.Should().NotBeNull();
            //responseDocument.SingleData.Attributes["description"].Should().Be(newDescription);
            //responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            //var newWorkItemId = int.Parse(responseDocument.SingleData.Id);

            //await _testContext.RunOnDatabaseAsync(async dbContext =>
            //{
                //var workItemInDatabase = await dbContext.WorkItems
                    //.Include(workItem => workItem.Assignee)
                    //.Include(workItem => workItem.Subscribers)
                    //.Include(workItem => workItem.WorkItemTags)
                    //.ThenInclude(workItemTag => workItemTag.Tag)
                    //.FirstAsync(workItem => workItem.Id == newWorkItemId);

                //workItemInDatabase.Description.Should().Be(newDescription);

                //workItemInDatabase.Assignee.Should().NotBeNull();
                //workItemInDatabase.Assignee.Id.Should().Be(existingUserAccounts[0].Id);

                //workItemInDatabase.Subscribers.Should().HaveCount(1);
                //workItemInDatabase.Subscribers.Single().Id.Should().Be(existingUserAccounts[1].Id);

                //workItemInDatabase.WorkItemTags.Should().HaveCount(1);
                //workItemInDatabase.WorkItemTags.Single().Tag.Id.Should().Be(existingTag.Id);
            //});
        //}
    }
}
