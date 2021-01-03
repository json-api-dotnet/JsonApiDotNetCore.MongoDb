using System;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.MongoDb.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.MongoDb.Queries.Expressions
{
    internal sealed class MongoDbQueryExpressionValidator : QueryExpressionRewriter<object>
    {
        public void Validate(QueryLayer layer)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            bool hasIncludes = layer.Include?.Elements.Any() == true;
            var hasSparseRelationshipSets = layer.Projection?.Any(pair => pair.Key is RelationshipAttribute) == true;

            if (hasIncludes || hasSparseRelationshipSets)
            {
                throw new UnsupportedRelationshipException();
            }

            ValidateExpression(layer.Filter);
            ValidateExpression(layer.Sort);
            ValidateExpression(layer.Pagination);
        }

        private void ValidateExpression(QueryExpression expression)
        {
            if (expression != null)
            {
                Visit(expression, null);
            }
        }

        public override QueryExpression VisitResourceFieldChain(ResourceFieldChainExpression expression, object argument)
        {
            if (expression != null)
            {
                if (expression.Fields.Count > 1 || expression.Fields.First() is RelationshipAttribute)
                {
                    throw new UnsupportedRelationshipException();
                }
            }

            return base.VisitResourceFieldChain(expression, argument);
        }

        public override QueryExpression VisitComparison(ComparisonExpression expression, object argument)
        {
            if (expression?.Left is ResourceFieldChainExpression && expression.Right is ResourceFieldChainExpression)
            {
                // https://jira.mongodb.org/browse/CSHARP-1592
                throw new UnsupportedComparisonExpressionException();
            }

            return base.VisitComparison(expression, argument);
        }
    }
}