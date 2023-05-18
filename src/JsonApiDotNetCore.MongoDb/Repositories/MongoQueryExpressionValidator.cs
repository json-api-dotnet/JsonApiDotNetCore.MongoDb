using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.MongoDb.Repositories;

internal sealed class MongoQueryExpressionValidator : QueryExpressionRewriter<object?>
{
    public void Validate(QueryLayer layer)
    {
        ArgumentGuard.NotNull(layer);

        bool hasIncludes = layer.Include?.Elements.Any() == true;

        if (hasIncludes || HasSparseRelationshipSets(layer.Selection))
        {
            throw new UnsupportedRelationshipException();
        }

        ValidateExpression(layer.Filter);
        ValidateExpression(layer.Sort);
        ValidateExpression(layer.Pagination);
    }

    private static bool HasSparseRelationshipSets(FieldSelection? selection)
    {
        if (selection is { IsEmpty: false })
        {
            foreach (ResourceType resourceType in selection.GetResourceTypes())
            {
                FieldSelectors selectors = selection.GetOrCreateSelectors(resourceType);

                if (selectors.Any(pair => pair.Key is RelationshipAttribute))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ValidateExpression(QueryExpression? expression)
    {
        if (expression != null)
        {
            Visit(expression, null);
        }
    }

    public override QueryExpression? VisitResourceFieldChain(ResourceFieldChainExpression expression, object? argument)
    {
        if (expression.Fields.Count > 1 || expression.Fields.First() is RelationshipAttribute)
        {
            throw new UnsupportedRelationshipException();
        }

        return base.VisitResourceFieldChain(expression, argument);
    }

    public override QueryExpression? VisitComparison(ComparisonExpression expression, object? argument)
    {
        if (expression is { Left: ResourceFieldChainExpression, Right: ResourceFieldChainExpression })
        {
            throw new AttributeComparisonInFilterNotSupportedException();
        }

        return base.VisitComparison(expression, argument);
    }
}
