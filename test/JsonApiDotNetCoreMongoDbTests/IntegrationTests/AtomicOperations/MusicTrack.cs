using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations")]
public sealed class MusicTrack : HexStringMongoIdentifiable
{
    [RegularExpression(@"^[a-fA-F\d]{24}$")]
    public override string? Id { get; set; }

    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    [Range(1, 24 * 60)]
    public decimal? LengthInSeconds { get; set; }

    [Attr]
    public string? Genre { get; set; }

    [Attr]
    public DateTimeOffset ReleasedAt { get; set; }

    [HasOne]
    [BsonIgnore]
    public Lyric? Lyric { get; set; }

    [HasOne]
    [BsonIgnore]
    public RecordCompany? OwnedBy { get; set; }

    [HasMany]
    [BsonIgnore]
    public IList<Performer> Performers { get; set; } = new List<Performer>();

    [HasMany]
    [BsonIgnore]
    public IList<Playlist> OccursIn { get; set; } = new List<Playlist>();
}
