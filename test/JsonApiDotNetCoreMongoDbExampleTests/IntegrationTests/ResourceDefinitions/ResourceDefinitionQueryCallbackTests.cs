using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ResourceDefinitions
{
    public class ResourceDefinitionQueryCallbackTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;

        public ResourceDefinitionQueryCallbackTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;

            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<IUserRolesService, FakeUserRolesService>();
            });

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Filter_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = false
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = false
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<CallableResource>();
                await db.GetCollection<CallableResource>().InsertManyAsync(resources);
            });

            var route = "/callableResources";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(resources[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[3].StringId);

            responseDocument.Meta["totalResources"].Should().Be(2);
        }

        [Fact]
        public async Task Filter_from_resource_definition_and_query_string_are_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = false
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = false
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<CallableResource>();
                await db.GetCollection<CallableResource>().InsertManyAsync(resources);
            });

            var route = "/callableResources?filter=equals(label,'B')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(resources[3].StringId);

            responseDocument.Meta["totalResources"].Should().Be(1);
        }

        [Fact]
        public async Task Sort_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.January(2001)
                },
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.December(2001)
                },
                new CallableResource
                {
                    Label = "B",
                    CreatedAt = 1.February(2001),
                    ModifiedAt = 15.January(2001)
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<CallableResource>();
                await db.GetCollection<CallableResource>().InsertManyAsync(resources);
            });

            var route = "/callableResources";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(resources[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(resources[2].StringId);
        }

        [Fact]
        public async Task Sort_from_query_string_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.January(2001)
                },
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.December(2001)
                },
                new CallableResource
                {
                    Label = "B",
                    CreatedAt = 1.February(2001),
                    ModifiedAt = 15.January(2001)
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<CallableResource>();
                await db.GetCollection<CallableResource>().InsertManyAsync(resources);
            });

            var route = "/callableResources?sort=-createdAt,modifiedAt";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(resources[2].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(resources[1].StringId);
        }

        [Fact]
        public async Task Page_size_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>();

            for (int index = 0; index < 10; index++)
            {
                resources.Add(new CallableResource());
            }

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<CallableResource>();
                await db.GetCollection<CallableResource>().InsertManyAsync(resources);
            });

            var route = "/callableResources?page[size]=8";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(5);
        }

        [Fact]
        public async Task Attribute_inclusion_from_resource_definition_is_applied_for_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                PercentageComplete = 5
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<CallableResource>().InsertOneAsync(resource);
            });

            var route = $"/callableResources/{resource.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Attributes["percentageComplete"].Should().Be(resource.PercentageComplete);
        }

        [Fact]
        public async Task Attribute_inclusion_from_resource_definition_is_applied_for_non_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                PercentageComplete = 5
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<CallableResource>().InsertOneAsync(resource);
            });

            var route = $"/callableResources/{resource.StringId}?fields[callableResources]=label,status";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(2);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Attributes["status"].Should().Be("5% completed.");
            responseDocument.SingleData.Relationships.Should().BeNull();
        }
        
        [Fact]
        public async Task Attribute_exclusion_from_resource_definition_is_applied_for_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                RiskLevel = 3
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<CallableResource>().InsertOneAsync(resource);
            });

            var route = $"/callableResources/{resource.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Attributes.Should().NotContainKey("riskLevel");
        }

        [Fact]
        public async Task Attribute_exclusion_from_resource_definition_is_applied_for_non_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                RiskLevel = 3
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<CallableResource>().InsertOneAsync(resource);
            });

            var route = $"/callableResources/{resource.StringId}?fields[callableResources]=label,riskLevel";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 8
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 8
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<CallableResource>();
                await db.GetCollection<CallableResource>().InsertManyAsync(resources);
            });

            var route = "/callableResources?isHighRisk=true";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(resources[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[3].StringId);
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_and_query_string_filter_are_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 8
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 8
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.ClearCollectionAsync<CallableResource>();
                await db.GetCollection<CallableResource>().InsertManyAsync(resources);
            });

            var route = "/callableResources?isHighRisk=false&filter=equals(label,'B')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(resources[2].StringId);
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_is_not_applied_on_secondary_request()
        {
            // Arrange
            var resource = new CallableResource
            {
                RiskLevel = 3,
                Children = new List<CallableResource>
                {
                    new CallableResource
                    {
                        RiskLevel = 3
                    },
                    new CallableResource
                    {
                        RiskLevel = 8
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                await db.GetCollection<CallableResource>().InsertOneAsync(resource);
            });

            var route = $"/callableResources/{resource.StringId}/children?isHighRisk=true";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Custom query string parameters cannot be used on nested resource endpoints.");
            responseDocument.Errors[0].Detail.Should().Be("Query string parameter 'isHighRisk' cannot be used on a nested resource endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("isHighRisk");
        }

        private sealed class FakeUserRolesService : IUserRolesService
        {
            public bool AllowIncludeOwner { get; set; } = true;
        }
    }
}