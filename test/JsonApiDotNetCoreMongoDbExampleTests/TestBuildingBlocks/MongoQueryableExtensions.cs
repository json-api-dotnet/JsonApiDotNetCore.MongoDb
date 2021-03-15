using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using MongoDB.Driver.Linq;

namespace JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks
{
    internal static class MongoQueryableExtensions
    {
        public static async Task<TResource> FirstWithIdAsync<TResource, TId>(this IMongoQueryable<TResource> resources, TId id,
            CancellationToken cancellationToken = default)
            where TResource : IIdentifiable<TId>
        {
            TResource firstOrDefault = await resources.FirstOrDefaultAsync(resource => Equals(resource.Id, id), cancellationToken);

            if (Equals(firstOrDefault, default(TResource)))
            {
                throw new InvalidOperationException($"Resource with ID '{id}' was not found.");
            }

            return firstOrDefault;
        }

        public static Task<TResource> FirstWithIdOrDefaultAsync<TResource, TId>(this IMongoQueryable<TResource> resources, TId id,
            CancellationToken cancellationToken = default)
            where TResource : IIdentifiable<TId>
        {
            return resources.FirstOrDefaultAsync(resource => Equals(resource.Id, id), cancellationToken);
        }
    }
}
