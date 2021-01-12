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
        public static IServiceCollection AddJsonApiMongoDb(this IServiceCollection services)
        {
            services.AddScoped<IResourceObjectBuilder, IgnoreRelationshipsResponseResourceObjectBuilder>();

            return services;
        }
    }
}
