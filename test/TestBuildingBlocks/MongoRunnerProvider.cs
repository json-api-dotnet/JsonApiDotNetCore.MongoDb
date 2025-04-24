using EphemeralMongo;

namespace TestBuildingBlocks;

// Based on https://gist.github.com/asimmon/612b2d54f1a0d2b4e1115590d456e0be.
internal sealed class MongoRunnerProvider
{
    public static readonly MongoRunnerProvider Instance = new();

#if NET8_0
    private readonly object _lockObject = new();
#else
    private readonly Lock _lockObject = new();
#endif

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
            ObjectDisposedException.ThrowIf(_underlyingMongoRunner == null, this);

            _underlyingMongoRunner.Import(database, collection, inputFilePath, additionalArguments, drop);
        }

        public void Export(string database, string collection, string outputFilePath, string? additionalArguments = null)
        {
            ObjectDisposedException.ThrowIf(_underlyingMongoRunner == null, this);

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
