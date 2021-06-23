using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.MongoDb.Serialization.Building
{
    /// <inheritdoc />
    public sealed class IgnoreRelationshipsResponseResourceObjectBuilder : ResponseResourceObjectBuilder
    {
        public IgnoreRelationshipsResponseResourceObjectBuilder(ILinkBuilder linkBuilder, IIncludedResourceObjectBuilder includedBuilder,
            IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceContextProvider resourceContextProvider,
            IResourceDefinitionAccessor resourceDefinitionAccessor, IResourceObjectBuilderSettingsProvider settingsProvider,
            IEvaluatedIncludeCache evaluatedIncludeCache)
            : base(linkBuilder, includedBuilder, constraintProviders, resourceContextProvider, resourceDefinitionAccessor, settingsProvider,
                evaluatedIncludeCache)
        {
        }

        /// <inheritdoc />
        protected override RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            if (resource is MongoIdentifiable)
            {
                return null;
            }

            return base.GetRelationshipData(relationship, resource);
        }
    }
}
