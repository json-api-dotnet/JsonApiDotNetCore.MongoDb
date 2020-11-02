using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace JsonApiDotNetCore.MongoDb.Extensions
{
    public static class MongoIQueryableExtensions
    {
        public static async Task<IReadOnlyList<T>> ToListAsync<T>(this IQueryable<T> queryable) =>
            await IAsyncCursorSourceExtensions.ToListAsync(ToMongoQueryable(queryable));

        public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> queryable) =>
            IAsyncCursorSourceExtensions.SingleOrDefaultAsync(ToMongoQueryable(queryable));

        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> queryable) =>
            IAsyncCursorSourceExtensions.FirstOrDefaultAsync(ToMongoQueryable(queryable));

        private static IMongoQueryable<T> ToMongoQueryable<T>(IQueryable<T> queryable) =>
            (queryable is IMongoQueryable<T> mongoQueryable) ?
                mongoQueryable :
                throw new ArgumentException($"This MongoDB-specific extension method expects a {nameof(IMongoQueryable<T>)} and cannot work with {nameof(IQueryable<T>)} of type {queryable.GetType().Name}.");
    }
}
