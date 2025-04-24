using System.Collections.Concurrent;
using System.Reflection;
using JsonApiDotNetCore.MongoDb;
using JsonApiDotNetCore.Resources;

#pragma warning disable AV1008 // Class should not be static

namespace TestBuildingBlocks;

internal static class ResourceTypeFinder
{
    private static readonly ConcurrentDictionary<Assembly, IReadOnlySet<Type>> ResourceTypesPerAssembly = new();
    private static readonly ConcurrentDictionary<string, IReadOnlySet<Type>> ResourceTypesPerNamespace = new();

    public static IReadOnlySet<Type> GetResourceClrTypesInNamespace(Assembly assembly, string? codeNamespace)
    {
        IReadOnlySet<Type> resourceClrTypesInAssembly = ResourceTypesPerAssembly.GetOrAdd(assembly, GetResourceClrTypesInAssembly);

        string namespaceKey = codeNamespace ?? string.Empty;
        return ResourceTypesPerNamespace.GetOrAdd(namespaceKey, _ => FilterTypesInNamespace(resourceClrTypesInAssembly, codeNamespace));
    }

    private static IReadOnlySet<Type> GetResourceClrTypesInAssembly(Assembly assembly)
    {
        return assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IIdentifiable))).ToHashSet().AsReadOnly();
    }

    private static IReadOnlySet<Type> FilterTypesInNamespace(IEnumerable<Type> resourceClrTypesInAssembly, string? codeNamespace)
    {
        return resourceClrTypesInAssembly.Where(resourceClrType => resourceClrType.Namespace == codeNamespace).ToHashSet().AsReadOnly();
    }
}
