using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;

namespace GettingStarted.Definitions;

public sealed class BooksDefinition : JsonApiResourceDefinition<Book, string?>
{
    private readonly IRequestQueryStringAccessor _queryStringAccessor;

    public BooksDefinition(IResourceGraph resourceGraph, IRequestQueryStringAccessor queryStringAccessor)
        : base(resourceGraph)
    {
        _queryStringAccessor = queryStringAccessor;
    }

    public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
    {
        if (!_queryStringAccessor.Query.ContainsKey("fields[books]"))
        {
            return existingSparseFieldSet
                .Excluding<Book>(book => book.Refs1, ResourceGraph)
                .Excluding<Book>(book => book.Refs2, ResourceGraph)
                .Excluding<Book>(book => book.Refs3, ResourceGraph);
        }

        return existingSparseFieldSet;
    }
}
