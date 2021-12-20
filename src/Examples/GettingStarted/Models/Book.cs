using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Book : HexStringMongoIdentifiable
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public string Author { get; set; } = null!;

    [Attr]
    public int PublishYear { get; set; }
}
