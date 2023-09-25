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
        private readonly JsonApiOptions _options;
        private readonly JsonApiOptions _backupValues;

        public JsonApiOptionsScope(IJsonApiOptions options)
        {
            _options = (JsonApiOptions)options;

            _backupValues = new JsonApiOptions
            {
                Namespace = options.Namespace,
                DefaultAttrCapabilities = options.DefaultAttrCapabilities,
                DefaultHasOneCapabilities = options.DefaultHasOneCapabilities,
                DefaultHasManyCapabilities = options.DefaultHasManyCapabilities,
                IncludeJsonApiVersion = options.IncludeJsonApiVersion,
                IncludeExceptionStackTraceInErrors = options.IncludeExceptionStackTraceInErrors,
                IncludeRequestBodyInErrors = options.IncludeRequestBodyInErrors,
                UseRelativeLinks = options.UseRelativeLinks,
                TopLevelLinks = options.TopLevelLinks,
                ResourceLinks = options.ResourceLinks,
                RelationshipLinks = options.RelationshipLinks,
                IncludeTotalResourceCount = options.IncludeTotalResourceCount,
                DefaultPageSize = options.DefaultPageSize,
                MaximumPageSize = options.MaximumPageSize,
                MaximumPageNumber = options.MaximumPageNumber,
                ValidateModelState = options.ValidateModelState,
                ClientIdGeneration = options.ClientIdGeneration,
                AllowUnknownQueryStringParameters = options.AllowUnknownQueryStringParameters,
                AllowUnknownFieldsInRequestBody = options.AllowUnknownFieldsInRequestBody,
                EnableLegacyFilterNotation = options.EnableLegacyFilterNotation,
                MaximumIncludeDepth = options.MaximumIncludeDepth,
                MaximumOperationsPerRequest = options.MaximumOperationsPerRequest,
                TransactionIsolationLevel = options.TransactionIsolationLevel
            };
        }

        public void Dispose()
        {
            _options.Namespace = _backupValues.Namespace;
            _options.DefaultAttrCapabilities = _backupValues.DefaultAttrCapabilities;
            _options.DefaultHasOneCapabilities = _backupValues.DefaultHasOneCapabilities;
            _options.DefaultHasManyCapabilities = _backupValues.DefaultHasManyCapabilities;
            _options.IncludeJsonApiVersion = _backupValues.IncludeJsonApiVersion;
            _options.IncludeExceptionStackTraceInErrors = _backupValues.IncludeExceptionStackTraceInErrors;
            _options.IncludeRequestBodyInErrors = _backupValues.IncludeRequestBodyInErrors;
            _options.UseRelativeLinks = _backupValues.UseRelativeLinks;
            _options.TopLevelLinks = _backupValues.TopLevelLinks;
            _options.ResourceLinks = _backupValues.ResourceLinks;
            _options.RelationshipLinks = _backupValues.RelationshipLinks;
            _options.IncludeTotalResourceCount = _backupValues.IncludeTotalResourceCount;
            _options.DefaultPageSize = _backupValues.DefaultPageSize;
            _options.MaximumPageSize = _backupValues.MaximumPageSize;
            _options.MaximumPageNumber = _backupValues.MaximumPageNumber;
            _options.ValidateModelState = _backupValues.ValidateModelState;
            _options.ClientIdGeneration = _backupValues.ClientIdGeneration;
            _options.AllowUnknownQueryStringParameters = _backupValues.AllowUnknownQueryStringParameters;
            _options.AllowUnknownFieldsInRequestBody = _backupValues.AllowUnknownFieldsInRequestBody;
            _options.EnableLegacyFilterNotation = _backupValues.EnableLegacyFilterNotation;
            _options.MaximumIncludeDepth = _backupValues.MaximumIncludeDepth;
            _options.MaximumOperationsPerRequest = _backupValues.MaximumOperationsPerRequest;
            _options.TransactionIsolationLevel = _backupValues.TransactionIsolationLevel;
        }
    }
}
