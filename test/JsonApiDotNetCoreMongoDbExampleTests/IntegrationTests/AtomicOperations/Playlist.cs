using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Playlist : MongoDbIdentifiable
    {
        [Attr]
        [Required]
        public string Name { get; set; }

        [Attr]
        [BsonIgnore]
        public bool IsArchived => false;

        [HasManyThrough(nameof(PlaylistMusicTracks))]
        [BsonIgnore]
        public IList<MusicTrack> Tracks { get; set; }

        [BsonIgnore]
        public IList<PlaylistMusicTrack> PlaylistMusicTracks { get; set; }
    }
}
