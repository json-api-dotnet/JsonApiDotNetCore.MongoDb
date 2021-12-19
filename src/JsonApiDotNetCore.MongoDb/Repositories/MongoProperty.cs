using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace JsonApiDotNetCore.MongoDb.Repositories;

internal sealed class MongoProperty : IProperty
{
    IReadOnlyEntityType IReadOnlyProperty.DeclaringEntityType => DeclaringEntityType;

    public IEntityType DeclaringEntityType { get; }
    public PropertyInfo? PropertyInfo { get; }

    public string Name => throw new NotImplementedException();
    public IReadOnlyTypeBase DeclaringType => throw new NotImplementedException();
    public Type ClrType => throw new NotImplementedException();
    public FieldInfo FieldInfo => throw new NotImplementedException();
    public bool IsNullable => throw new NotImplementedException();
    public ValueGenerated ValueGenerated => throw new NotImplementedException();
    public bool IsConcurrencyToken => throw new NotImplementedException();
    public object this[string name] => throw new NotImplementedException();

    public MongoProperty(PropertyInfo propertyInfo, MongoEntityType owner)
    {
        ArgumentGuard.NotNull(owner, nameof(owner));
        ArgumentGuard.NotNull(propertyInfo, nameof(propertyInfo));

        DeclaringEntityType = owner;
        PropertyInfo = propertyInfo;
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

    public IClrPropertyGetter GetGetter()
    {
        throw new NotImplementedException();
    }

    public IComparer<IUpdateEntry> GetCurrentValueComparer()
    {
        throw new NotImplementedException();
    }

    public CoreTypeMapping FindTypeMapping()
    {
        throw new NotImplementedException();
    }

    public int? GetMaxLength()
    {
        throw new NotImplementedException();
    }

    public int? GetPrecision()
    {
        throw new NotImplementedException();
    }

    public int? GetScale()
    {
        throw new NotImplementedException();
    }

    public bool? IsUnicode()
    {
        throw new NotImplementedException();
    }

    public PropertySaveBehavior GetBeforeSaveBehavior()
    {
        throw new NotImplementedException();
    }

    public PropertySaveBehavior GetAfterSaveBehavior()
    {
        throw new NotImplementedException();
    }

    public Func<IProperty, IEntityType, ValueGenerator> GetValueGeneratorFactory()
    {
        throw new NotImplementedException();
    }

    public ValueConverter GetValueConverter()
    {
        throw new NotImplementedException();
    }

    public Type GetProviderClrType()
    {
        throw new NotImplementedException();
    }

    ValueComparer IProperty.GetValueComparer()
    {
        throw new NotImplementedException();
    }

    ValueComparer IProperty.GetKeyValueComparer()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IKey> GetContainingKeys()
    {
        throw new NotImplementedException();
    }

    ValueComparer IReadOnlyProperty.GetValueComparer()
    {
        throw new NotImplementedException();
    }

    ValueComparer IReadOnlyProperty.GetKeyValueComparer()
    {
        throw new NotImplementedException();
    }

    public bool IsForeignKey()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IForeignKey> GetContainingForeignKeys()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IIndex> GetContainingIndexes()
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyForeignKey> IReadOnlyProperty.GetContainingForeignKeys()
    {
        return GetContainingForeignKeys();
    }

    public bool IsIndex()
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyIndex> IReadOnlyProperty.GetContainingIndexes()
    {
        return GetContainingIndexes();
    }

    public IReadOnlyKey FindContainingPrimaryKey()
    {
        throw new NotImplementedException();
    }

    public bool IsKey()
    {
        throw new NotImplementedException();
    }

    IEnumerable<IReadOnlyKey> IReadOnlyProperty.GetContainingKeys()
    {
        return GetContainingKeys();
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
