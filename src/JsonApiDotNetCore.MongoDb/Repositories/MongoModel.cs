using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Repositories;

internal sealed class MongoModel : IModel
{
    private readonly IResourceGraph _resourceGraph;

    public object this[string name] => throw new NotImplementedException();

    public MongoModel(IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

        _resourceGraph = resourceGraph;
    }

    public IEnumerable<IEntityType> GetEntityTypes()
    {
        IReadOnlySet<ResourceType> resourceTypes = _resourceGraph.GetResourceTypes();
        return resourceTypes.Select(resourceType => new MongoEntityType(resourceType, this)).ToArray();
    }

    public IAnnotation FindAnnotation(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IAnnotation> GetAnnotations()
    {
        throw new NotImplementedException();
    }

    public ChangeTrackingStrategy GetChangeTrackingStrategy()
    {
        throw new NotImplementedException();
    }

    public PropertyAccessMode GetPropertyAccessMode()
    {
        throw new NotImplementedException();
    }

    public bool IsShared(Type type)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyEntityType> IReadOnlyModel.GetEntityTypes()
    {
        return GetEntityTypes();
    }

    public IEntityType FindEntityType(string name)
    {
        throw new NotImplementedException();
    }

    public IEntityType FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
    {
        throw new NotImplementedException();
    }

    IReadOnlyEntityType IReadOnlyModel.FindEntityType(string name)
    {
        return FindEntityType(name);
    }

    public IReadOnlyEntityType FindEntityType(string name, string definingNavigationName, IReadOnlyEntityType definingEntityType)
    {
        throw new NotImplementedException();
    }

    public IEntityType FindEntityType(Type type)
    {
        throw new NotImplementedException();
    }

    IReadOnlyEntityType IReadOnlyModel.FindEntityType(Type type)
    {
        return FindEntityType(type);
    }

    public IReadOnlyEntityType FindEntityType(Type type, string definingNavigationName, IReadOnlyEntityType definingEntityType)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEntityType> FindEntityTypes(Type type)
    {
        throw new NotImplementedException();
    }

    public bool IsIndexerMethod(MethodInfo methodInfo)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ITypeMappingConfiguration> GetTypeMappingConfigurations()
    {
        throw new NotImplementedException();
    }

    public ITypeMappingConfiguration FindTypeMappingConfiguration(Type scalarType)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyEntityType> IReadOnlyModel.FindEntityTypes(Type type)
    {
        return FindEntityTypes(type);
    }

    public IAnnotation FindRuntimeAnnotation(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IAnnotation> GetRuntimeAnnotations()
    {
        throw new NotImplementedException();
    }

    public IAnnotation AddRuntimeAnnotation(string name, object? value)
    {
        throw new NotImplementedException();
    }

    public IAnnotation SetRuntimeAnnotation(string name, object? value)
    {
        throw new NotImplementedException();
    }

    public IAnnotation RemoveRuntimeAnnotation(string name)
    {
        throw new NotImplementedException();
    }

    public TValue GetOrAddRuntimeAnnotationValue<TValue, TArg>(string name, Func<TArg?, TValue> valueFactory, TArg? factoryArgument)
    {
        throw new NotImplementedException();
    }
}
