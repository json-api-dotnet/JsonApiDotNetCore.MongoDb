using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class QueryStringDbContext : MongoDbContextShim
{
    public MongoDbSetShim<Blog> Blogs => Set<Blog>();
    public MongoDbSetShim<BlogPost> Posts => Set<BlogPost>();
    public MongoDbSetShim<WebAccount> Accounts => Set<WebAccount>();

    public QueryStringDbContext(IMongoDatabase database)
        : base(database)
    {
    }
}
