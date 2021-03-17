using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.MongoDb.Serialization.Building;
using JsonApiDotNetCore.Serialization.Building;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.MongoDb.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Expands JsonApiDotNetCore configuration for usage with MongoDB.
        /// </summary>
        [PublicAPI]
        public static IServiceCollection AddJsonApiMongoDb(this IServiceCollection services)
        {
            services.AddScoped<IMongoDataAccess, MongoDataAccess>();
            services.AddScoped<IOperationsTransactionFactory, MongoDbTransactionFactory>();
            services.AddScoped<IResourceObjectBuilder, IgnoreRelationshipsResponseResourceObjectBuilder>();

            return services;
        }
    }
}
