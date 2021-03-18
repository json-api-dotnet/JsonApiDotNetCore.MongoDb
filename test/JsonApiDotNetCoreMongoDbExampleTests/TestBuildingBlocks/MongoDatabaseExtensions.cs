using System.Threading.Tasks;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks
{
    internal static class MongoDatabaseExtensions
    {
        public static IMongoCollection<TResource> GetCollection<TResource>(this IMongoDatabase database)
        {
            return database.GetCollection<TResource>(typeof(TResource).Name);
        }

        public static Task ClearCollectionAsync<TResource>(this IMongoDatabase database)
        {
            return database.DropCollectionAsync(typeof(TResource).Name);
        }

        public static async Task EnsureEmptyCollectionAsync<TResource>(this IMongoDatabase database)
        {
            await database.DropCollectionAsync(typeof(TResource).Name);
            await database.CreateCollectionAsync(typeof(TResource).Name);
        }

        public static Task InsertManyAsync<TDocument>(this IMongoCollection<TDocument> collection, params TDocument[] documents)
        {
            return collection.InsertManyAsync(documents);
        }
    }
}
