using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class FailingPerformerRepository(
    IMongoDataAccess mongoDataAccess, ITargetedFields targetedFields, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder,
    OperationsTransientFailureSimulator failureSimulator)
    : MongoRepository<Performer, string?>(mongoDataAccess, targetedFields, resourceGraph, resourceFactory, constraintProviders, resourceDefinitionAccessor,
        queryableBuilder)
{
    private readonly OperationsTransientFailureSimulator _failureSimulator = failureSimulator;

    protected override async Task<TResult> SaveChangesAsync<TResult>(Func<Task<TResult>> asyncSaveAction, CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(async () =>
        {
            _failureSimulator.FailAsConfigured();
            return await asyncSaveAction();
        }, cancellationToken);
    }
}
