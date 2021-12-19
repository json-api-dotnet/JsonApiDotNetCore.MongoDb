namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.ResourceDefinitions.Reading;

internal sealed class TestClientSettingsProvider : IClientSettingsProvider
{
    public bool ArePlanetsWithPrivateNameHidden { get; private set; }

    public void ResetToDefaults()
    {
        ArePlanetsWithPrivateNameHidden = false;
    }

    public void HidePlanetsWithPrivateName()
    {
        ArePlanetsWithPrivateNameHidden = true;
    }
}
