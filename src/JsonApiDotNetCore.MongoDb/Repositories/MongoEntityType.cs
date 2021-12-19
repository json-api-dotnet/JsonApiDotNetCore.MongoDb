using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Repositories;

internal sealed class MongoEntityType : IEntityType
{
    private readonly ResourceType _resourceType;

    IReadOnlyModel IReadOnlyTypeBase.Model => Model;
    IReadOnlyEntityType IReadOnlyEntityType.BaseType => BaseType;

    public IModel Model { get; }
    public Type ClrType => _resourceType.ClrType;

    public string Name => throw new NotImplementedException();
    public bool HasSharedClrType => throw new NotImplementedException();
    public bool IsPropertyBag => throw new NotImplementedException();
    public IEntityType BaseType => throw new NotImplementedException();
    public InstantiationBinding ConstructorBinding => throw new NotImplementedException();
    public object this[string name] => throw new NotImplementedException();

    public MongoEntityType(ResourceType resourceType, MongoModel owner)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));
        ArgumentGuard.NotNull(owner, nameof(owner));

        _resourceType = resourceType;
        Model = owner;
    }

    public IEnumerable<IProperty> GetProperties()
    {
        return _resourceType.Attributes.Select(attr => new MongoProperty(attr.Property, this)).ToArray();
    }

    public IAnnotation FindAnnotation(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IAnnotation> GetAnnotations()
    {
        throw new NotImplementedException();
    }

    public PropertyAccessMode GetPropertyAccessMode()
    {
        throw new NotImplementedException();
    }

    public PropertyAccessMode GetNavigationAccessMode()
    {
        throw new NotImplementedException();
    }

    public PropertyInfo FindIndexerPropertyInfo()
    {
        throw new NotImplementedException();
    }

    public ChangeTrackingStrategy GetChangeTrackingStrategy()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IDictionary<string, object?>> GetSeedData(bool providerValues = false)
    {
        throw new NotImplementedException();
    }

    public LambdaExpression GetQueryFilter()
    {
        throw new NotImplementedException();
    }

    public string GetDiscriminatorPropertyName()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IReadOnlyEntityType> GetDerivedTypes()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEntityType> GetDirectlyDerivedTypes()
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDirectlyDerivedTypes()
    {
        return GetDirectlyDerivedTypes();
    }

    IReadOnlyKey IReadOnlyEntityType.FindPrimaryKey()
    {
        return FindPrimaryKey();
    }

    public IKey FindKey(IReadOnlyList<IReadOnlyProperty> properties)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IKey> GetDeclaredKeys()
    {
        throw new NotImplementedException();
    }

    public IForeignKey FindForeignKey(IReadOnlyList<IReadOnlyProperty> properties, IReadOnlyKey principalKey, IReadOnlyEntityType principalEntityType)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IKey> GetKeys()
    {
        throw new NotImplementedException();
    }

    public IKey FindPrimaryKey()
    {
        throw new NotImplementedException();
    }

    IReadOnlyKey IReadOnlyEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
    {
        return FindKey(properties);
    }

    IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetKeys()
    {
        return GetKeys();
    }

    IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetDeclaredKeys()
    {
        return GetDeclaredKeys();
    }

    IReadOnlyForeignKey IReadOnlyEntityType.FindForeignKey(IReadOnlyList<IReadOnlyProperty> properties, IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
    {
        return FindForeignKey(properties, principalKey, principalEntityType);
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
    {
        return FindForeignKeys(properties);
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
    {
        return FindDeclaredForeignKeys(properties);
    }

    public IEnumerable<IForeignKey> GetDeclaredForeignKeys()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IForeignKey> GetDerivedForeignKeys()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IForeignKey> GetForeignKeys()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IForeignKey> GetReferencingForeignKeys()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IForeignKey> GetDeclaredReferencingForeignKeys()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IForeignKey> FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredForeignKeys()
    {
        return GetDeclaredForeignKeys();
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDerivedForeignKeys()
    {
        return GetDerivedForeignKeys();
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetForeignKeys()
    {
        return GetForeignKeys();
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetReferencingForeignKeys()
    {
        return GetReferencingForeignKeys();
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredReferencingForeignKeys()
    {
        return GetDeclaredReferencingForeignKeys();
    }

    IReadOnlyNavigation IReadOnlyEntityType.FindDeclaredNavigation(string name)
    {
        return FindDeclaredNavigation(name);
    }

    public IEnumerable<INavigation> GetDeclaredNavigations()
    {
        throw new NotImplementedException();
    }

    public INavigation FindDeclaredNavigation(string name)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDeclaredNavigations()
    {
        return GetDeclaredNavigations();
    }

    public IEnumerable<IReadOnlyNavigation> GetDerivedNavigations()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<INavigation> GetNavigations()
    {
        throw new NotImplementedException();
    }

    public ISkipNavigation FindSkipNavigation(string name)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetNavigations()
    {
        return GetNavigations();
    }

    IReadOnlySkipNavigation IReadOnlyEntityType.FindSkipNavigation(string name)
    {
        return FindSkipNavigation(name);
    }

    public IEnumerable<IReadOnlySkipNavigation> GetDeclaredSkipNavigations()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IReadOnlySkipNavigation> GetDerivedSkipNavigations()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISkipNavigation> GetSkipNavigations()
    {
        throw new NotImplementedException();
    }

    public IIndex FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
    {
        throw new NotImplementedException();
    }

    public IIndex FindIndex(string name)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetSkipNavigations()
    {
        return GetSkipNavigations();
    }

    IReadOnlyIndex IReadOnlyEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
    {
        return FindIndex(properties);
    }

    IReadOnlyIndex IReadOnlyEntityType.FindIndex(string name)
    {
        return FindIndex(name);
    }

    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDeclaredIndexes()
    {
        return GetDeclaredIndexes();
    }

    public IEnumerable<IIndex> GetDerivedIndexes()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IIndex> GetIndexes()
    {
        throw new NotImplementedException();
    }

    public IProperty FindProperty(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IIndex> GetDeclaredIndexes()
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDerivedIndexes()
    {
        return GetDerivedIndexes();
    }

    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetIndexes()
    {
        return GetIndexes();
    }

    IReadOnlyProperty IReadOnlyEntityType.FindProperty(string name)
    {
        return FindProperty(name);
    }

    public IReadOnlyList<IReadOnlyProperty> FindProperties(IReadOnlyList<string> propertyNames)
    {
        throw new NotImplementedException();
    }

    public IProperty FindDeclaredProperty(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IProperty> GetDeclaredProperties()
    {
        throw new NotImplementedException();
    }

    IReadOnlyProperty IReadOnlyEntityType.FindDeclaredProperty(string name)
    {
        return FindDeclaredProperty(name);
    }

    IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetDeclaredProperties()
    {
        return GetDeclaredProperties();
    }

    public IEnumerable<IReadOnlyProperty> GetDerivedProperties()
    {
        throw new NotImplementedException();
    }

    public IServiceProperty FindServiceProperty(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IServiceProperty> GetDeclaredServiceProperties()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IProperty> GetForeignKeyProperties()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IProperty> GetValueGeneratingProperties()
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetProperties()
    {
        return GetProperties();
    }

    IReadOnlyServiceProperty IReadOnlyEntityType.FindServiceProperty(string name)
    {
        return FindServiceProperty(name);
    }

    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDeclaredServiceProperties()
    {
        return GetDeclaredServiceProperties();
    }

    public IEnumerable<IReadOnlyServiceProperty> GetDerivedServiceProperties()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IServiceProperty> GetServiceProperties()
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetServiceProperties()
    {
        return GetServiceProperties();
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
