using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Drives conversion from <see cref="QueryLayer"/> into system <see cref="Expression"/> trees.
    /// </summary>
    public sealed class QueryableBuilder
    {
        private readonly Expression _source;
        private readonly Type _elementType;
        private readonly Type _extensionType;
        private readonly LambdaParameterNameFactory _nameFactory;
        private readonly IResourceFactory _resourceFactory;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly LambdaScopeFactory _lambdaScopeFactory;

        public QueryableBuilder(Expression source, Type elementType, Type extensionType, LambdaParameterNameFactory nameFactory,
            IResourceFactory resourceFactory, IResourceContextProvider resourceContextProvider,
            LambdaScopeFactory lambdaScopeFactory = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _elementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            _extensionType = extensionType ?? throw new ArgumentNullException(nameof(extensionType));
            _nameFactory = nameFactory ?? throw new ArgumentNullException(nameof(nameFactory));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _lambdaScopeFactory = lambdaScopeFactory ?? new LambdaScopeFactory(_nameFactory);
        }

        public Expression ApplyQuery(QueryLayer layer)
        {
            layer = layer ?? throw new ArgumentNullException(nameof(layer));

            Expression expression = _source;

            if (layer.Filter != null)
            {
                expression = ApplyFilter(expression, layer.Filter);
            }

            if (layer.Sort != null)
            {
                expression = ApplySort(expression, layer.Sort);
            }

            if (layer.Pagination != null)
            {
                expression = ApplyPagination(expression, layer.Pagination);
            }

            return expression;
        }

        private Expression ApplyFilter(Expression source, FilterExpression filter)
        {
            using var lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new WhereClauseBuilder(source, lambdaScope, _extensionType);
            return builder.ApplyWhere(filter);
        }

        private Expression ApplySort(Expression source, SortExpression sort)
        {
            using var lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new OrderClauseBuilder(source, lambdaScope, _extensionType);
            return builder.ApplyOrderBy(sort);
        }

        private Expression ApplyPagination(Expression source, PaginationExpression pagination)
        {
            using var lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new SkipTakeClauseBuilder(source, lambdaScope, _extensionType);
            return builder.ApplySkipTake(pagination);
        }
    }
}
