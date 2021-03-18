using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Drives conversion from <see cref="QueryLayer" /> into system <see cref="Expression" /> trees.
    /// </summary>
    [PublicAPI]
    public sealed class MongoQueryableBuilder : QueryableBuilder
    {
        private readonly Type _elementType;
        private readonly Type _extensionType;
        private readonly LambdaScopeFactory _lambdaScopeFactory;

        public MongoQueryableBuilder(Expression source, Type elementType, Type extensionType, LambdaParameterNameFactory nameFactory,
            IResourceFactory resourceFactory, IResourceContextProvider resourceContextProvider, IModel entityModel,
            LambdaScopeFactory lambdaScopeFactory = null)
            : base(source, elementType, extensionType, nameFactory, resourceFactory, resourceContextProvider, entityModel, lambdaScopeFactory)
        {
            ArgumentGuard.NotNull(elementType, nameof(elementType));
            ArgumentGuard.NotNull(extensionType, nameof(extensionType));

            _elementType = elementType;
            _extensionType = extensionType;
            _lambdaScopeFactory = lambdaScopeFactory ?? new LambdaScopeFactory(nameFactory);
        }

        protected override Expression ApplyFilter(Expression source, FilterExpression filter)
        {
            using LambdaScope lambdaScope = _lambdaScopeFactory.CreateScope(_elementType);

            var builder = new MongoWhereClauseBuilder(source, lambdaScope, _extensionType);
            return builder.ApplyWhere(filter);
        }
    }
}
