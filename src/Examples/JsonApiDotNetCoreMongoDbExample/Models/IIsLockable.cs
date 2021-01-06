namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public interface IIsLockable
    {
        bool IsLocked { get; set; }
    }
}