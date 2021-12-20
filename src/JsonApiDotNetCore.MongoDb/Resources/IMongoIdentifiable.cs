using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.MongoDb.Resources;

/// <summary>
/// Marker interface to indicate a resource that is stored in MongoDB.
/// </summary>
public interface IMongoIdentifiable : IIdentifiable<string?>
{
}
