using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.MongoDb.Repositories
{
    internal sealed class MongoProperty : IProperty
    {
        public IEntityType DeclaringEntityType { get; }
        public PropertyInfo PropertyInfo { get; }

        public string Name => throw new NotImplementedException();
        public Type ClrType => throw new NotImplementedException();
        public FieldInfo FieldInfo => throw new NotImplementedException();
        public ITypeBase DeclaringType => throw new NotImplementedException();
        public bool IsNullable => throw new NotImplementedException();
        public ValueGenerated ValueGenerated => throw new NotImplementedException();
        public bool IsConcurrencyToken => throw new NotImplementedException();
        public object this[string name] => throw new NotImplementedException();

        public MongoProperty(PropertyInfo propertyInfo, MongoEntityType owner)
        {
            ArgumentGuard.NotNull(owner, nameof(owner));
            ArgumentGuard.NotNull(propertyInfo, nameof(propertyInfo));

            DeclaringEntityType = owner;
            PropertyInfo = propertyInfo;
        }

        public IAnnotation FindAnnotation(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAnnotation> GetAnnotations()
        {
            throw new NotImplementedException();
        }
    }
}
