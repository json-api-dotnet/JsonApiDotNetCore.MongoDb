using JsonApiDotNetCore.MongoDb.Repositories;
using MongoDB.Driver;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.AtomicOperations;

/// <summary>
/// Used to simulate transient database failures during <see cref="MongoRepository{TResource,TId}.SaveChangesAsync" />.
/// </summary>
public sealed class OperationsTransientFailureSimulator
{
    internal bool FailOnNextAttempt { get; set; }
    internal bool UseTransientError { get; set; }
    internal int AttemptCount { get; private set; }

    internal void Reset()
    {
        FailOnNextAttempt = false;
        UseTransientError = false;
        AttemptCount = 0;
    }

    internal void FailAsConfigured()
    {
        AttemptCount++;

        if (FailOnNextAttempt)
        {
            FailOnNextAttempt = false;

            var exception = new MongoException("Simulated database failure.");

            if (UseTransientError)
            {
                exception.AddErrorLabel("TransientTransactionError");
            }

            throw exception;
        }
    }
}
