using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Book : MongoIdentifiable
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public string Author { get; set; } = null!;

    [Attr]
    public int PublishYear { get; set; }

    [Attr]
    public List<string>? Refs1 { get; set; }

    [Attr]
    public List<string>? Refs2 { get; set; }

    [Attr]
    public List<string>? Refs3 { get; set; }
}
