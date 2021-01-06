﻿using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreMongoDbExample;
using JsonApiDotNetCoreMongoDbExample.Definitions;
using JsonApiDotNetCoreMongoDbExample.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Meta
{
    public sealed class ResourceMetaTests : IClassFixture<IntegrationTestContext<Startup>>
    {
        private readonly IntegrationTestContext<Startup> _testContext;

        public ResourceMetaTests(IntegrationTestContext<Startup> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<IResourceDefinition<TodoItem, string>, TodoItemDefinition>();
            });
        }

        [Fact]
        public async Task ResourceDefinition_That_Implements_GetMeta_Contains_Resource_Meta()
        {
            // Arrange
            var todoItems = new[]
            {
                new TodoItem {Description = "Important: Pay the bills"},
                new TodoItem {Description = "Plan my birthday party"},
                new TodoItem {Description = "Important: Call mom"}
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<TodoItem>(nameof(TodoItem));
                await collection.DeleteManyAsync(Builders<TodoItem>.Filter.Empty);

                foreach (var todoItem in todoItems)
                {
                    await collection.InsertOneAsync(todoItem);
                }
            });

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Meta.Should().ContainKey("hasHighPriority");
            responseDocument.ManyData[1].Meta.Should().BeNull();
            responseDocument.ManyData[2].Meta.Should().ContainKey("hasHighPriority");
        }
    }
}
