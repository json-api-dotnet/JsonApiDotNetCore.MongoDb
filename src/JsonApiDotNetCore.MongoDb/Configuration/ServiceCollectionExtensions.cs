using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.MongoDb.AtomicOperations;
using JsonApiDotNetCore.MongoDb.Queries.Internal;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries.Internal;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection

namespace JsonApiDotNetCore.MongoDb.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Expands JsonApiDotNetCore configuration for usage with MongoDB.
    /// </summary>
    [PublicAPI]
    public static IServiceCollection AddJsonApiMongoDb(this IServiceCollection services)
    {
        services.AddScoped<IMongoDataAccess, MongoDataAccess>();
        services.AddScoped<IOperationsTransactionFactory, MongoTransactionFactory>();
        services.AddScoped<ISparseFieldSetCache, HideRelationshipsSparseFieldSetCache>();

        return services;
    }
}
