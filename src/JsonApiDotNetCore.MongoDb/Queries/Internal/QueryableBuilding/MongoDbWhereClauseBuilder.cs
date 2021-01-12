using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

namespace JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding
{
    /// <inheritdoc />
    public class MongoDbWhereClauseBuilder : WhereClauseBuilder
    {
        public MongoDbWhereClauseBuilder(Expression source, LambdaScope lambdaScope, Type extensionType) : base(source,
            lambdaScope, extensionType)
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
            var convertedValue = Convert.ChangeType(value, expressionType);
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
