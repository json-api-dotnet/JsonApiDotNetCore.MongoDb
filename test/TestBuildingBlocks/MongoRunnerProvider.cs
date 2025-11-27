using EphemeralMongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using LockPrimitive =
#if NET9_0_OR_GREATER
    System.Threading.Lock
#else
    object
#endif
    ;

namespace TestBuildingBlocks;

// Based on https://gist.github.com/asimmon/612b2d54f1a0d2b4e1115590d456e0be.
internal sealed class MongoRunnerProvider
{
    private static readonly GuidSerializer StandardGuidSerializer = new(GuidRepresentation.Standard);
    public static readonly MongoRunnerProvider Instance = new();

    private readonly LockPrimitive _lockObject = new();
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
                BsonSerializer.TryRegisterSerializer(StandardGuidSerializer);

                var runnerOptions = new MongoRunnerOptions
                {
                    // Single-node replica set mode is required for transaction support in MongoDB.
                    UseSingleNodeReplicaSet = true,
                    AdditionalArguments = ["--quiet"]
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

        public string ConnectionString
        {
            get
            {
                ObjectDisposedException.ThrowIf(_underlyingMongoRunner == null, typeof(IMongoRunner));
                return _underlyingMongoRunner.ConnectionString;
            }
        }

        public void Import(string database, string collection, string inputFilePath, string[]? additionalArguments = null, bool drop = false,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_underlyingMongoRunner == null, typeof(IMongoRunner));

            _underlyingMongoRunner.Import(database, collection, inputFilePath, additionalArguments, drop, cancellationToken);
        }

        public async Task ImportAsync(string database, string collection, string inputFilePath, string[]? additionalArguments = null, bool drop = false,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_underlyingMongoRunner == null, typeof(IMongoRunner));

            await _underlyingMongoRunner.ImportAsync(database, collection, inputFilePath, additionalArguments, drop, cancellationToken);
        }

        public void Export(string database, string collection, string outputFilePath, string[]? additionalArguments = null,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_underlyingMongoRunner == null, typeof(IMongoRunner));

            _underlyingMongoRunner.Export(database, collection, outputFilePath, additionalArguments, cancellationToken);
        }

        public async Task ExportAsync(string database, string collection, string outputFilePath, string[]? additionalArguments = null,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_underlyingMongoRunner == null, typeof(IMongoRunner));

            await _underlyingMongoRunner.ExportAsync(database, collection, outputFilePath, additionalArguments, cancellationToken);
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
