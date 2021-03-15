using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Resources
{
    /// <summary>
    /// A convenient basic implementation of <see cref="IIdentifiable" /> for use with MongoDB models.
    /// </summary>
    public abstract class MongoDbIdentifiable : IIdentifiable<string>
    {
        /// <inheritdoc />
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public virtual string Id { get; set; }

        /// <inheritdoc />
        [BsonIgnore]
        public string StringId
        {
            get => Id;
            set => Id = value;
        }
    }
}
