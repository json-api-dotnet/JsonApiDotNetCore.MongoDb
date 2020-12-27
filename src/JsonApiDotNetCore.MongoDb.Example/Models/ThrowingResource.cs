using System;
using System.Diagnostics;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public sealed class ThrowingResource : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr]
        public string FailsOnSerialize
        {
            get
            {
                var isSerializingResponse = new StackTrace().GetFrames()
                    .Any(frame => frame.GetMethod().DeclaringType == typeof(JsonApiWriter));
                
                if (isSerializingResponse)
                {
                    throw new InvalidOperationException($"The value for the '{nameof(FailsOnSerialize)}' property is currently unavailable.");
                }

                return string.Empty;
            }
            set { }
        }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
