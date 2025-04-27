using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Configuration;

internal static class ResourceGraphExtensions
{
    public static IReadOnlyModel ToEntityModel(this IResourceGraph resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(resourceGraph);

        var modelBuilder = new ModelBuilder();

        foreach (ResourceType resourceType in resourceGraph.GetResourceTypes())
        {
            IncludeResourceType(resourceType, modelBuilder);
        }

        return modelBuilder.Model;
    }

    private static void IncludeResourceType(ResourceType resourceType, ModelBuilder builder)
    {
        EntityTypeBuilder entityTypeBuilder = builder.Entity(resourceType.ClrType);

        foreach (PropertyInfo property in resourceType.ClrType.GetProperties().Where(property => !IsIgnored(property)))
        {
            entityTypeBuilder.Property(property.PropertyType, property.Name);
        }
    }

    private static bool IsIgnored(PropertyInfo property)
    {
        return property.GetCustomAttribute<BsonIgnoreAttribute>() != null;
    }
}
