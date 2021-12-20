using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Resources;

/// <summary>
/// Basic implementation of a JSON:API resource whose Id is stored as a 12-byte hexadecimal ObjectId in MongoDB.
/// </summary>
public abstract class HexStringMongoIdentifiable : IMongoIdentifiable
{
    /// <inheritdoc />
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
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
