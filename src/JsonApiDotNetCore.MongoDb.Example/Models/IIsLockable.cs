namespace JsonApiDotNetCore.MongoDb.Example.Models
{
    public interface IIsLockable
    {
        bool IsLocked { get; set; }
    }
}