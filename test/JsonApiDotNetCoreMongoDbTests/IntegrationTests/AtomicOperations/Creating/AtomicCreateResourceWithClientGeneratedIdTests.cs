using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Creating;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicCreateResourceWithClientGeneratedIdTests
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicCreateResourceWithClientGeneratedIdTests(AtomicOperationsFixture fixture)
    {
        _testContext = fixture.TestContext;

        var options = (JsonApiOptions)fixture.TestContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowClientGeneratedIds = true;
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_string_ID_having_side_effects()
    {
        // Arrange
        TextLanguage newLanguage = _fakers.TextLanguage.Generate();
        newLanguage.Id = "free-format-client-generated-id";

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "textLanguages",
                        id = newLanguage.StringId,
                        attributes = new
                        {
                            isoCode = newLanguage.IsoCode
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        string isoCode = $"{newLanguage.IsoCode}{ContainerTypeToHideFromAutoDiscovery.ImplicitlyChangingTextLanguageDefinition.Suffix}";

        responseDocument.Results.ShouldHaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("textLanguages");
            resource.Attributes.ShouldContainKey("isoCode").With(value => value.Should().Be(isoCode));
            resource.Attributes.Should().NotContainKey("isRightToLeft");
            resource.Relationships.Should().BeNull();
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextLanguage languageInDatabase = await dbContext.TextLanguages.FirstWithIdAsync(newLanguage.Id);

            languageInDatabase.IsoCode.Should().Be(isoCode);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
    {
        // Arrange
        Playlist newPlaylist = _fakers.Playlist.Generate();
        newPlaylist.Id = "free-format-client-generated-id";

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "playlists",
                        id = newPlaylist.StringId,
                        attributes = new
                        {
                            name = newPlaylist.Name
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Playlist playlistInDatabase = await dbContext.Playlists.FirstWithIdAsync(newPlaylist.Id);

            playlistInDatabase.Name.Should().Be(newPlaylist.Name);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_for_existing_client_generated_ID()
    {
        // Arrange
        TextLanguage existingLanguage = _fakers.TextLanguage.Generate();
        existingLanguage.Id = "existing-free-format-client-generated-id";

        string newIsoCode = _fakers.TextLanguage.Generate().IsoCode!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextLanguages.Add(existingLanguage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "textLanguages",
                        id = existingLanguage.StringId,
                        attributes = new
                        {
                            isoCode = newIsoCode
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'textLanguages' with ID '{existingLanguage.StringId}' already exists.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }
}
