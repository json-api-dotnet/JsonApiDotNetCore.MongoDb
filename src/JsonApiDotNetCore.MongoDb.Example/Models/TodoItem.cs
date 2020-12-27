using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public class TodoItem : IIdentifiable<string>, IIsLockable
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Attr]
        public string Id { get; set; }
        
        public bool IsLocked { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public long Ordinal { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowCreate)]
        public string AlwaysChangingValue
        {
            get => Guid.NewGuid().ToString();
            set { }
        }

        [Attr]
        public DateTime CreatedDate { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
        public DateTime? AchievedDate { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public string CalculatedValue => "calculated";

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowChange)]
        public DateTimeOffset? OffsetDate { get; set; }
 
        // [HasOne]
        // public Person Owner { get; set; }
        //
        // [HasOne]
        // public Person Assignee { get; set; }
        //
        // [HasOne]
        // public Person OneToOnePerson { get; set; }
        //
        // [HasMany]
        // public ISet<Person> StakeHolders { get; set; }
        //
        // [HasOne]
        // public TodoItemCollection Collection { get; set; }
        //
        // // cyclical to-one structure
        // [HasOne]
        // public TodoItem DependentOnTodo { get; set; }
        //
        // // cyclical to-many structure
        // [HasOne]
        // public TodoItem ParentTodo { get; set; }
        //
        // [HasMany]
        // public IList<TodoItem> ChildrenTodos { get; set; }
        
        [BsonIgnore]
        public string StringId { get => Id; set => Id = value; }
    }
}
