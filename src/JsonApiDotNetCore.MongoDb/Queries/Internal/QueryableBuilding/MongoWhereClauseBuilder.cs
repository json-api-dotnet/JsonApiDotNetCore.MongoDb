using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

namespace JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding
{
    /// <inheritdoc />
    [PublicAPI]
    public class MongoWhereClauseBuilder : WhereClauseBuilder
    {
        public MongoWhereClauseBuilder(Expression source, LambdaScope lambdaScope, Type extensionType, LambdaParameterNameFactory nameFactory)
            : base(source, lambdaScope, extensionType, nameFactory)
        {
        }

        public override Expression VisitLiteralConstant(LiteralConstantExpression expression, Type expressionType)
        {
            if (expressionType == typeof(DateTime) || expressionType == typeof(DateTime?))
            {
                DateTime? dateTime = TryParseDateTimeAsUtc(expression.Value, expressionType);
                return Expression.Constant(dateTime);
            }

            return base.VisitLiteralConstant(expression, expressionType);
        }

        private static DateTime? TryParseDateTimeAsUtc(string value, Type expressionType)
        {
            object convertedValue = Convert.ChangeType(value, expressionType);

            if (convertedValue is DateTime dateTime)
            {
                // DateTime values in MongoDB are always stored in UTC, so any ambiguous filter value passed
                // must be interpreted as such for correct comparison.
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }

                return dateTime;
            }

            return null;
        }
    }
}
