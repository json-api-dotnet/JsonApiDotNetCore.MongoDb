using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Repositories;

internal sealed class MongoModel : IModel
{
    private readonly IResourceContextProvider _resourceContextProvider;

    public object this[string name] => throw new NotImplementedException();

    public MongoModel(IResourceContextProvider resourceContextProvider)
    {
        ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));

        _resourceContextProvider = resourceContextProvider;
    }

    public IEnumerable<IEntityType> GetEntityTypes()
    {
        IReadOnlyCollection<ResourceContext> resourceContexts = _resourceContextProvider.GetResourceContexts();
        return resourceContexts.Select(resourceContext => new MongoEntityType(resourceContext, this)).ToArray();
    }

    public IAnnotation FindAnnotation(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IAnnotation> GetAnnotations()
    {
        throw new NotImplementedException();
    }

    public IEntityType FindEntityType(string name)
    {
        throw new NotImplementedException();
    }

    public IEntityType FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
    {
        throw new NotImplementedException();
    }
}
