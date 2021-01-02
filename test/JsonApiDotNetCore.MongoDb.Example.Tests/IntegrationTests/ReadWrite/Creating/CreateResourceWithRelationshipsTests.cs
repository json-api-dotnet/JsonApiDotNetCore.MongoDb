using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceWithRelationshipsTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();
        
        public CreateResourceWithRelationshipsTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<RgbColor, string>();
                builder.Add<UserAccount, string>();
                builder.Add<WorkItem, string>();
                builder.Add<WorkItemGroup, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoDbRepository<RgbColor>>();
                services.AddResourceRepository<MongoDbRepository<UserAccount>>();
                services.AddResourceRepository<MongoDbRepository<WorkItem>>();
                services.AddResourceRepository<MongoDbRepository<WorkItemGroup>>();
            });

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Cannot_create_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            var color = _fakers.RgbColor.Generate();
            var group = _fakers.WorkItemGroup.Generate();
            group.Color = color;

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<RgbColor>(nameof(RgbColor)).InsertOneAsync(color);
                await db.GetCollection<WorkItemGroup>(nameof(WorkItemGroup)).InsertOneAsync(group);
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    relationships = new
                    {
                        color = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = group.Color.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
        }

        [Fact]
        public async Task Cannot_create_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            var color = _fakers.RgbColor.Generate();
            var group = _fakers.WorkItemGroup.Generate();
            group.Id = ObjectId.GenerateNewId().ToString();
            group.Color = color;
            color.Group = group;

            await _testContext.RunOnDatabaseAsync(async db =>
                await db.GetCollection<RgbColor>(nameof(RgbColor)).InsertOneAsync(color));

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = ObjectId.GenerateNewId().ToString(),
                    relationships = new
                    {
                        group = new
                        {
                            data = new
                            {
                                type = "workItemGroups",
                                id = color.Group.StringId
                            }
                        }
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
        }

        [Fact]
        public async Task Cannot_create_relationship_with_include()
        {
            // Arrange
            var existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async db =>
                await db.GetCollection<UserAccount>(nameof(UserAccount)).InsertOneAsync(existingUserAccount));

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccount.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItems?include=assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
        }

        [Fact]
        public async Task Cannot_create_HasMany_relationship()
        {
            // Arrange
            var existingUserAccounts = _fakers.UserAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async db =>
                await db.GetCollection<UserAccount>(nameof(UserAccount)).InsertManyAsync(existingUserAccounts));

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[0].StringId
                                },
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[1].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Relationships are not supported when using MongoDB.");
        }
    }
}