using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TodoItem : MongoDbIdentifiable
    {
        [Attr]
        public string Description { get; set; }

        [Attr]
        public DateTimeOffset CreatedAt { get; set; }

        [Attr]
        public DateTimeOffset? CompletedAt { get; set; }
    }
}
