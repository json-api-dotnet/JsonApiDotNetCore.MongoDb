﻿using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Drives conversion from <see cref="QueryLayer"/> into system <see cref="Expression"/> trees.
    /// </summary>
    public sealed class MongoDbQueryableBuilder : QueryableBuilder
    {
        private readonly Type _elementType;
        private readonly Type _extensionType;
        private readonly LambdaScopeFactory _lambdaScopeFactory;

        public MongoDbQueryableBuilder(Expression source, Type elementType, Type extensionType,
            LambdaParameterNameFactory nameFactory, IResourceFactory resourceFactory,
            IResourceContextProvider resourceContextProvider, IModel entityModel,
            LambdaScopeFactory lambdaScopeFactory = null)
            : base(source, elementType, extensionType, nameFactory, resourceFactory, resourceContextProvider,
                entityModel, lambdaScopeFactory)
        {
            _elementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            _extensionType = extensionType ?? throw new ArgumentNullException(nameof(extensionType));
            _lambdaScopeFactory = lambdaScopeFactory ?? new LambdaScopeFactory(nameFactory);
        }

        protected override Expression ApplyFilter(Expression source, FilterExpression filter)
        {
            using var lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new MongoDbWhereClauseBuilder(source, lambdaScope, _extensionType);
            return builder.ApplyWhere(filter);
        }
    }
}
