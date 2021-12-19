using JetBrains.Annotations;
using MongoDB.Driver;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OperationsDbContext : MongoDbContextShim
{
    public MongoDbSetShim<Playlist> Playlists => Set<Playlist>();
    public MongoDbSetShim<MusicTrack> MusicTracks => Set<MusicTrack>();
    public MongoDbSetShim<Lyric> Lyrics => Set<Lyric>();
    public MongoDbSetShim<TextLanguage> TextLanguages => Set<TextLanguage>();
    public MongoDbSetShim<Performer> Performers => Set<Performer>();
    public MongoDbSetShim<RecordCompany> RecordCompanies => Set<RecordCompany>();

    public OperationsDbContext(IMongoDatabase database)
        : base(database)
    {
    }
}
