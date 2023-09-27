using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

public abstract class BaseForAtomicOperationsTestsThatChangeOptions : IDisposable
{
    private readonly JsonApiOptionsScope _optionsScope;

    protected BaseForAtomicOperationsTestsThatChangeOptions(AtomicOperationsFixture fixture)
    {
        var options = (JsonApiOptions)fixture.TestContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        _optionsScope = new JsonApiOptionsScope(options);
    }

    public void Dispose()
    {
        _optionsScope.Dispose();
    }

    private sealed class JsonApiOptionsScope : IDisposable
    {
        private static readonly List<PropertyInfo> PropertyCache = typeof(JsonApiOptions).GetProperties().Where(IsAccessibleProperty).ToList();

        private readonly JsonApiOptions _options;
        private readonly JsonApiOptions _backupValues;

        public JsonApiOptionsScope(JsonApiOptions options)
        {
            _options = options;
            _backupValues = new JsonApiOptions();

            CopyPropertyValues(_options, _backupValues);
        }

        private static bool IsAccessibleProperty(PropertyInfo property)
        {
            return property.GetMethod != null && property.SetMethod != null && property.GetCustomAttribute<ObsoleteAttribute>() == null;
        }

        public void Dispose()
        {
            CopyPropertyValues(_backupValues, _options);
        }

        private static void CopyPropertyValues(JsonApiOptions source, JsonApiOptions destination)
        {
            foreach (PropertyInfo property in PropertyCache)
            {
                property.SetMethod!.Invoke(destination, new[]
                {
                    property.GetMethod!.Invoke(source, null)
                });
            }
        }
    }
}
