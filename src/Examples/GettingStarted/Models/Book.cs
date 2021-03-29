using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace GettingStarted.Models
{
    // TODO: Remove limitation in repository that using Int32 as Id is wrong. As long as you provide a proper IIdGenerator, this just works. Change tests to use various Id types.
    // TODO: Make MongoIdentifiable.Id non-virtual and remove [BsonId]. Make MongoIdentifiable to implement IMongoIdentifiable<string>.

    public interface IMongoIdentifiable<TId> : IIdentifiable<TId>
    {
        // We cannot provide a base class, because the MongoDB driver won't allow to override Id to put attributes on it.
        // See https://stackoverflow.com/questions/7228609/mongodb-c-sharp-driver-can-a-field-called-id-not-be-id.
        // So the next best thing is to provide an interface with default implementations where possible.

        /// <inheritdoc />
        [BsonIgnore]
        string IIdentifiable.StringId
        {
            get => EqualityComparer<TId>.Default.Equals(Id, default) ? null : Id.ToString();
            set => Id = value == null ? default : (TId)Convert.ChangeType(value, typeof(TId)); // TODO: use RuntimeTypeConverter for GUID support.
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Book : IMongoIdentifiable<string>
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        [BsonIgnore]
        public string LocalId { get; set; }

        [Attr]
        public string Title { get; set; }

        [Attr]
        public int PublishYear { get; set; }

        // TODO: Change into List<Author> (and add tests for complex objects)
        [Attr]
        public string Author { get; set; }
    }
}
