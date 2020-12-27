using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public class Passport : IIdentifiable<string>
    {
        // private readonly ISystemClock _systemClock;
        private int? _socialSecurityNumber;

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [Attr]
        public int? SocialSecurityNumber
        {
            get => _socialSecurityNumber;
            set
            {
                if (value != _socialSecurityNumber)
                {
                    LastSocialSecurityNumberChange = DateTime.UtcNow.ToLocalTime();
                    _socialSecurityNumber = value;
                }
            }
        }

        [Attr]
        public DateTime LastSocialSecurityNumberChange { get; set; }

        [Attr]
        public bool IsLocked { get; set; }

        // [HasOne]
        // public Person Person { get; set; }

        // [Attr]
        // [NotMapped]
        // public string BirthCountryName
        // {
        //     get => BirthCountry?.Name;
        //     set
        //     {
        //         BirthCountry ??= new Country();
        //         BirthCountry.Name = value;
        //     }
        // }

        // [EagerLoad]
        // public Country BirthCountry { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }

        // public Passport(AppDbContext appDbContext)
        // {
        //     _systemClock = appDbContext.SystemClock;
        // }
    }
}
