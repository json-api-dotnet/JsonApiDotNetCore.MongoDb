﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Drives conversion from <see cref="QueryLayer"/> into system <see cref="Expression"/> trees.
    /// </summary>
    /// <remarks>
    /// This class was copied from JsonApiDotNetCore, so it can use <see cref="MongoDbWhereClauseBuilder"/> instead.
    /// </remarks>
    public sealed class MongoDbQueryableBuilder
    {
        private readonly Expression _source;
        private readonly Type _elementType;
        private readonly Type _extensionType;
        private readonly LambdaParameterNameFactory _nameFactory;
        private readonly IResourceFactory _resourceFactory;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IModel _entityModel;
        private readonly LambdaScopeFactory _lambdaScopeFactory;

        public MongoDbQueryableBuilder(Expression source, Type elementType, Type extensionType, LambdaParameterNameFactory nameFactory,
            IResourceFactory resourceFactory, IResourceContextProvider resourceContextProvider, IModel entityModel,
            LambdaScopeFactory lambdaScopeFactory = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _elementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            _extensionType = extensionType ?? throw new ArgumentNullException(nameof(extensionType));
            _nameFactory = nameFactory ?? throw new ArgumentNullException(nameof(nameFactory));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _entityModel = entityModel ?? throw new ArgumentNullException(nameof(entityModel));
            _lambdaScopeFactory = lambdaScopeFactory ?? new LambdaScopeFactory(_nameFactory);
        }

        public Expression ApplyQuery(QueryLayer layer)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            Expression expression = _source;

            if (layer.Include != null)
            {
                expression = ApplyInclude(expression, layer.Include, layer.ResourceContext);
            }

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

            if (layer.Projection != null && layer.Projection.Any())
            {
                expression = ApplyProjection(expression, layer.Projection, layer.ResourceContext);
            }

            return expression;
        }

        private Expression ApplyInclude(Expression source, IncludeExpression include, ResourceContext resourceContext)
        {
            using var lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new IncludeClauseBuilder(source, lambdaScope, resourceContext, _resourceContextProvider);
            return builder.ApplyInclude(include);
        }

        private Expression ApplyFilter(Expression source, FilterExpression filter)
        {
            using var lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new MongoDbWhereClauseBuilder(source, lambdaScope, _extensionType);
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

        private Expression ApplyProjection(Expression source, IDictionary<ResourceFieldAttribute, QueryLayer> projection, ResourceContext resourceContext)
        {
            using var lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new SelectClauseBuilder(source, lambdaScope, _entityModel, _extensionType, _nameFactory, _resourceFactory, _resourceContextProvider);
            return builder.ApplySelect(projection, resourceContext);
        }
    }
}
