using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings")]
public sealed class WebAccount : HexStringMongoIdentifiable
{
    [Attr]
    public string UserName { get; set; } = null!;

    [Attr(Capabilities = ~AttrCapabilities.AllowView)]
    public string Password { get; set; } = null!;

    [Attr]
    public string DisplayName { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
    public DateTime? DateOfBirth { get; set; }

    [Attr]
    public string EmailAddress { get; set; } = null!;

    [HasMany]
    [BsonIgnore]
    public IList<BlogPost> Posts { get; set; } = new List<BlogPost>();

    [HasOne]
    [BsonIgnore]
    public AccountPreferences? Preferences { get; set; }

    [HasMany]
    [BsonIgnore]
    public IList<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
}
