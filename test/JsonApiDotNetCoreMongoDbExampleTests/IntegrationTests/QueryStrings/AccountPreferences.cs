using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AccountPreferences : MongoIdentifiable
    {
        [Attr]
        public bool UseDarkTheme { get; set; }
    }
}
