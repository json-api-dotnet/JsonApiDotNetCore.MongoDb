using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Queries;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JsonApiDotNetCore.MongoDb.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Expands JsonApiDotNetCore configuration for usage with MongoDB.
    /// </summary>
    [PublicAPI]
    public static IServiceCollection AddJsonApiMongoDb(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(serviceProvider =>
        {
            var resourceGraph = serviceProvider.GetRequiredService<IResourceGraph>();
            return resourceGraph.ToEntityModel();
        });

        services.TryAddScoped<IMongoDataAccess, MongoDataAccess>();

        // Replace the built-in implementations from JsonApiDotNetCore.
        services.AddScoped<IOperationsTransactionFactory, MongoTransactionFactory>();
        services.AddScoped<ISparseFieldSetCache, HideRelationshipsSparseFieldSetCache>();

        return services;
    }
}
