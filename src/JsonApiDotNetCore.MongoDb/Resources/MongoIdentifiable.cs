using JsonApiDotNetCore.Resources;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Resources;

/// <summary>
/// A convenient basic implementation of <see cref="IIdentifiable" /> for use with MongoDB models.
/// </summary>
public abstract class MongoIdentifiable : IIdentifiable<string?>
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
