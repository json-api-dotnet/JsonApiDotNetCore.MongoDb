using System;
using MongoDB.Driver;

namespace JsonApiDotNetCore.MongoDb.Repositories
{
    /// <summary>
    /// Provides access to the MongoDB Driver and the optionally active session.
    /// </summary>
    public interface IMongoDataAccess : IAsyncDisposable
    {
        /// <summary>
        /// Provides access to the underlying MongoDB database, which data changes can be applied on.
        /// </summary>
        IMongoDatabase MongoDatabase { get; }

        /// <summary>
        /// Provides access to the active session, if any.
        /// </summary>
        IClientSessionHandle ActiveSession { get; set; }

        /// <summary>
        /// Identifies the current transaction, if any.
        /// </summary>
        string TransactionId { get; }
    }
}
