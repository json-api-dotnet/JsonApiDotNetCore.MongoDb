using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace JsonApiDotNetCore.MongoDb.Resources;

/// <summary>
/// Basic implementation of a JSON:API resource whose Id is stored as a free-format string in MongoDB. Useful for resources that are created using
/// client-generated IDs.
/// </summary>
public abstract class FreeStringMongoIdentifiable : IMongoIdentifiable
{
    /// <inheritdoc />
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public virtual string? Id { get; set; }

    /// <inheritdoc />
    [BsonIgnore]
    public string? StringId
    {
        get => Id;
        set => Id = value;
    }

    /// <inheritdoc />
    [BsonIgnore]
    public string? LocalId { get; set; }
}
