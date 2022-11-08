using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCore.MongoDb.Repositories;

internal sealed class MongoModel : RuntimeModel
{
    public MongoModel(IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph);

        foreach (ResourceType resourceType in resourceGraph.GetResourceTypes())
        {
            RuntimeEntityType entityType = AddEntityType(resourceType.ClrType.FullName!, resourceType.ClrType);
            SetEntityProperties(entityType, resourceType);
        }
    }

    private static void SetEntityProperties(RuntimeEntityType entityType, ResourceType resourceType)
    {
        foreach (PropertyInfo property in resourceType.ClrType.GetProperties().Where(property => !IsIgnored(property)))
        {
            entityType.AddProperty(property.Name, property.PropertyType, property);
        }
    }

    private static bool IsIgnored(PropertyInfo property)
    {
        return property.GetCustomAttribute<BsonIgnoreAttribute>() != null;
    }
}
