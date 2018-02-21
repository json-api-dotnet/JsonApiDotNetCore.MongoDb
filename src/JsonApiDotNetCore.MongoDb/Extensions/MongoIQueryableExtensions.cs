namespace JsonApiDotNetCore.MongoDb.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    public static class MongoIQueryableExtensions
    {
        public static async Task<IReadOnlyList<T>> ToListAsync<T>(this IQueryable<T> queryable)
        {
            return await IAsyncCursorSourceExtensions.ToListAsync(ToMongoQueryable(queryable));
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> queryable)
        {
            return await IAsyncCursorSourceExtensions.SingleOrDefaultAsync(ToMongoQueryable(queryable));
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> queryable)
        {
            return await IAsyncCursorSourceExtensions.FirstOrDefaultAsync(ToMongoQueryable(queryable));
        }

        private static IMongoQueryable<T> ToMongoQueryable<T>(IQueryable<T> queryable)
        {
            if (!(queryable is IMongoQueryable<T> mongoQueryable))
            {
                throw new ArgumentException($"This MongoDB-specific extension method expects a {nameof(IMongoQueryable<T>)} and cannot work with {nameof(IQueryable<T>)} of type {queryable.GetType().Name}.");
            }

            return mongoQueryable;
        }
    }
}
