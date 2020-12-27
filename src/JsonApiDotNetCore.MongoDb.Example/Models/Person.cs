using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public sealed class PersonRole : IIdentifiable<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        [HasOne]
        public Person Person { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }

    public sealed class Person : IIdentifiable<string>, IIsLockable
    {
        private string _firstName;

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        public bool IsLocked { get; set; }

        [Attr]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (value != _firstName)
                {
                    _firstName = value;
                    Initials = string.Concat(value.Split(' ').Select(x => char.ToUpperInvariant(x[0])));
                }
            }
        }

        [Attr]
        public string Initials { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr(PublicName = "the-Age")]
        public int Age { get; set; }

        [Attr]
        public Gender Gender { get; set; }

        [Attr]
        public string Category { get; set; }

        // [HasMany]
        // public ISet<TodoItem> TodoItems { get; set; }
        //
        // [HasMany]
        // public ISet<TodoItem> AssignedTodoItems { get; set; }
        //
        // [HasMany]
        // public HashSet<TodoItemCollection> TodoCollections { get; set; }
        //
        // [HasOne]
        // public PersonRole Role { get; set; }
        //
        // [HasOne]
        // public TodoItem OneToOneTodoItem { get; set; }
        //
        // [HasOne]
        // public TodoItem StakeHolderTodoItem { get; set; }
        //
        // [HasOne(Links = LinkTypes.All, CanInclude = false)]
        // public TodoItem UnIncludeableItem { get; set; }
        //
        // [HasOne]
        // public Passport Passport { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
