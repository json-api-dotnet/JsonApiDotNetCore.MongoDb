using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Creating;

[Collection("AtomicOperationsFixture")]
public sealed class AtomicCreateResourceWithClientGeneratedIdTests : BaseForAtomicOperationsTestsThatChangeOptions
{
    private readonly IntegrationTestContext<TestableStartup, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicCreateResourceWithClientGeneratedIdTests(AtomicOperationsFixture fixture)
        : base(fixture)
    {
        _testContext = fixture.TestContext;

        var options = (JsonApiOptions)fixture.TestContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = ClientIdGenerationMode.Required;
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_string_ID_having_side_effects()
    {
        // Arrange
        TextLanguage newLanguage = _fakers.TextLanguage.GenerateOne();
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string isoCode = $"{newLanguage.IsoCode}{ImplicitlyChangingTextLanguageDefinition.Suffix}";

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("textLanguages");
            resource.Attributes.Should().ContainKey("isoCode").WhoseValue.Should().Be(isoCode);
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
        Playlist newPlaylist = _fakers.Playlist.GenerateOne();
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

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
        TextLanguage existingLanguage = _fakers.TextLanguage.GenerateOne();
        existingLanguage.Id = "existing-free-format-client-generated-id";

        string newIsoCode = _fakers.TextLanguage.GenerateOne().IsoCode!;

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'textLanguages' with ID '{existingLanguage.StringId}' already exists.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }
}
