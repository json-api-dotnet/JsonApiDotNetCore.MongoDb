using System.Threading.Tasks;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbExampleTests
{
    public static class MongoDatabaseExtensions
    {
        public static IMongoCollection<TResource> GetCollection<TResource>(this IMongoDatabase db)
        {
            return db.GetCollection<TResource>(typeof(TResource).Name);
        }

        public static async Task ClearCollectionAsync<TResource>(this IMongoDatabase db)
        {
            var collection = GetCollection<TResource>(db);
            await collection.DeleteManyAsync(Builders<TResource>.Filter.Empty);
        }
    }
}
