using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public class User : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        // private readonly ISystemClock _systemClock;
        private string _password;

        [Attr] public string UserName { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                    LastPasswordChange = DateTime.UtcNow.ToLocalTime();
                }
            }
        }

        [Attr] public DateTime LastPasswordChange { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
