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

        public static async Task ClearCollectionAsync<TResource>(this IMongoDatabase database)
        {
            IMongoCollection<TResource> collection = GetCollection<TResource>(database);
            await collection.DeleteManyAsync(Builders<TResource>.Filter.Empty);
        }

        public static Task InsertManyAsync<TDocument>(this IMongoCollection<TDocument> collection, params TDocument[] documents)
        {
            return collection.InsertManyAsync(documents);
        }
    }
}
