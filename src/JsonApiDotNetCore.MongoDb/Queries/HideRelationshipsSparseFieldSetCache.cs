using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.MongoDb.Queries;

/// <inheritdoc cref="ISparseFieldSetCache" />
public sealed class HideRelationshipsSparseFieldSetCache : ISparseFieldSetCache
{
    private readonly SparseFieldSetCache _innerCache;

    public HideRelationshipsSparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        ArgumentNullException.ThrowIfNull(constraintProviders);
        ArgumentNullException.ThrowIfNull(resourceDefinitionAccessor);

        _innerCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        return _innerCache.GetSparseFieldSetForQuery(resourceType);
    }

    /// <inheritdoc />
    public IImmutableSet<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        return _innerCache.GetIdAttributeSetForRelationshipQuery(resourceType);
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        IImmutableSet<ResourceFieldAttribute> fieldSet = _innerCache.GetSparseFieldSetForSerializer(resourceType);

        return resourceType.ClrType.IsAssignableTo(typeof(IMongoIdentifiable)) ? RemoveRelationships(fieldSet) : fieldSet;
    }

    private static IImmutableSet<ResourceFieldAttribute> RemoveRelationships(IImmutableSet<ResourceFieldAttribute> fieldSet)
    {
        ResourceFieldAttribute[] relationships = fieldSet.Where(field => field is RelationshipAttribute).ToArray();
        return fieldSet.Except(relationships);
    }

    /// <inheritdoc />
    public void Reset()
    {
        _innerCache.Reset();
    }
}
