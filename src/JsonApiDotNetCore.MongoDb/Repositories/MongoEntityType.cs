using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Repositories
{
    internal sealed class MongoEntityType : IEntityType
    {
        private readonly ResourceContext _resourceContext;

        public IModel Model { get; }
        public Type ClrType => _resourceContext.ResourceType;

        public string Name => throw new NotImplementedException();
        public IEntityType BaseType => throw new NotImplementedException();
        public string DefiningNavigationName => throw new NotImplementedException();
        public IEntityType DefiningEntityType => throw new NotImplementedException();
        public object this[string name] => throw new NotImplementedException();

        public MongoEntityType(ResourceContext resourceContext, MongoModel owner)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));
            ArgumentGuard.NotNull(owner, nameof(owner));

            _resourceContext = resourceContext;
            Model = owner;
        }

        public IEnumerable<IProperty> GetProperties()
        {
            return _resourceContext.Attributes.Select(attr => new MongoProperty(attr.Property, this)).ToArray();
        }

        public IAnnotation FindAnnotation(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAnnotation> GetAnnotations()
        {
            throw new NotImplementedException();
        }

        public IKey FindPrimaryKey()
        {
            throw new NotImplementedException();
        }

        public IKey FindKey(IReadOnlyList<IProperty> properties)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IKey> GetKeys()
        {
            throw new NotImplementedException();
        }

        public IForeignKey FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IForeignKey> GetForeignKeys()
        {
            throw new NotImplementedException();
        }

        public IIndex FindIndex(IReadOnlyList<IProperty> properties)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IIndex> GetIndexes()
        {
            throw new NotImplementedException();
        }

        public IProperty FindProperty(string name)
        {
            throw new NotImplementedException();
        }

        public IServiceProperty FindServiceProperty(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IServiceProperty> GetServiceProperties()
        {
            throw new NotImplementedException();
        }
    }
}
