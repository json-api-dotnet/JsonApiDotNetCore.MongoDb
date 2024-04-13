using EphemeralMongo;

namespace TestBuildingBlocks;

// Based on https://gist.github.com/asimmon/612b2d54f1a0d2b4e1115590d456e0be.
internal sealed class MongoRunnerProvider
{
    public static readonly MongoRunnerProvider Instance = new();

    private readonly object _lockObject = new();
    private IMongoRunner? _runner;
    private int _useCounter;

    private MongoRunnerProvider()
    {
    }

    public IMongoRunner Get()
    {
        lock (_lockObject)
        {
            if (_runner == null)
            {
                var runnerOptions = new MongoRunnerOptions
                {
                    // Single-node replica set mode is required for transaction support in MongoDB.
                    UseSingleNodeReplicaSet = true,
                    KillMongoProcessesWhenCurrentProcessExits = true,
                    AdditionalArguments = "--quiet"
                };

                _runner = MongoRunner.Run(runnerOptions);
            }

            _useCounter++;
            return new MongoRunnerWrapper(this, _runner);
        }
    }

    private void Detach()
    {
        lock (_lockObject)
        {
            if (_runner != null)
            {
                _useCounter--;

                if (_useCounter == 0)
                {
                    _runner.Dispose();
                    _runner = null;
                }
            }
        }
    }

    private sealed class MongoRunnerWrapper(MongoRunnerProvider owner, IMongoRunner underlyingMongoRunner) : IMongoRunner
    {
        private readonly MongoRunnerProvider _owner = owner;
        private IMongoRunner? _underlyingMongoRunner = underlyingMongoRunner;

        public string ConnectionString => _underlyingMongoRunner?.ConnectionString ?? throw new ObjectDisposedException(nameof(IMongoRunner));

        public void Import(string database, string collection, string inputFilePath, string? additionalArguments = null, bool drop = false)
        {
            if (_underlyingMongoRunner == null)
            {
                throw new ObjectDisposedException(nameof(IMongoRunner));
            }

            _underlyingMongoRunner.Import(database, collection, inputFilePath, additionalArguments, drop);
        }

        public void Export(string database, string collection, string outputFilePath, string? additionalArguments = null)
        {
            if (_underlyingMongoRunner == null)
            {
                throw new ObjectDisposedException(nameof(IMongoRunner));
            }

            _underlyingMongoRunner.Export(database, collection, outputFilePath, additionalArguments);
        }

        public void Dispose()
        {
            if (_underlyingMongoRunner != null)
            {
                _underlyingMongoRunner = null;
                _owner.Detach();
            }
        }
    }
}
