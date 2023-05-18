using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class TodoItem : HexStringMongoIdentifiable
{
    [Attr]
    public string Description { get; set; } = null!;

    [Attr]
    [Required]
    public TodoItemPriority? Priority { get; set; }

    [Attr]
    public long? DurationInHours { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
    public DateTimeOffset CreatedAt { get; set; }

    [Attr(PublicName = "modifiedAt", Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
    public DateTimeOffset? LastModifiedAt { get; set; }
}
