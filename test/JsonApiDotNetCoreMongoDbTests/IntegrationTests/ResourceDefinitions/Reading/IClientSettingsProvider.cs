namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

public interface IClientSettingsProvider
{
    bool ArePlanetsWithPrivateNameHidden { get; }
}
