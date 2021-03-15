namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ResourceDefinitions
{
    public interface IUserRolesService
    {
        bool AllowIncludeOwner { get; }
    }
}
