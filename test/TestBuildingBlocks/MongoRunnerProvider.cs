using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using EphemeralMongo;
using FluentAssertions.Extensions;
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

    static MongoRunnerProvider()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            lock (Instance._lockObject)
            {
                Instance._runner?.Dispose();
                Instance._runner = null;
            }
        };
    }

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
                _runner = StartRunnerWithRetry();
            }

            return new MongoRunnerWrapper(_runner);
        }
    }

    private static IMongoRunner StartRunnerWithRetry()
    {
        const int maxAttempts = 2;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var logBuffer = new ConcurrentQueue<string>();
            var stopwatch = Stopwatch.StartNew();

            var runnerOptions = new MongoRunnerOptions
            {
                // Single-node replica set mode is required for transaction support in MongoDB.
                UseSingleNodeReplicaSet = true,
                // Restrict oplog size to 64 MB (MongoDB default is 5% of free disk space, which can exceed 15 GB on CI).
                AdditionalArguments =
                [
                    "--oplogSize",
                    "64"
                ],
                StandardOutputLogger = line => logBuffer.Enqueue($"[STDOUT] {line}"),
                StandardErrorLogger = line => logBuffer.Enqueue($"[STDERR] {line}")
            };

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
            {
                runnerOptions.ConnectionTimeout = 60.Seconds();
                runnerOptions.ReplicaSetSetupTimeout = 60.Seconds();
            }

            try
            {
                IMongoRunner runner = MongoRunner.Run(runnerOptions);

                Console.WriteLine(
                    $"[MongoRunnerProvider] Started MongoDB replica set on {runner.ConnectionString} in {stopwatch.ElapsedMilliseconds} ms (attempt {attempt}/{maxAttempts}).");

                return runner;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                string diagnosticReport = BuildDiagnosticReport(ex, attempt, maxAttempts, stopwatch.Elapsed, logBuffer);
                Console.Error.WriteLine(diagnosticReport);

                if (attempt == maxAttempts)
                {
                    throw ex is TimeoutException
                        ? new TimeoutException($"{ex.Message}\n\n{diagnosticReport}", ex)
                        : new InvalidOperationException($"Failed to start MongoDB replica set.\n\n{diagnosticReport}", ex);
                }

                Thread.Sleep(500);
            }
        }

        throw new InvalidOperationException("Unreachable.");
    }

    private static string BuildDiagnosticReport(Exception exception, int attempt, int maxAttempts, TimeSpan elapsed, ConcurrentQueue<string> logBuffer)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            $"=== [MongoRunnerProvider] Failed to start MongoDB replica set (attempt {attempt}/{maxAttempts}) in {elapsed.TotalSeconds:F1}s ===");

        builder.AppendLine($"Error: {exception.GetType().FullName}: {exception.Message}");
        builder.AppendLine($"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
        builder.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
        builder.AppendLine($"Process: PID {Environment.ProcessId}, Processors: {Environment.ProcessorCount}");

        try
        {
            string tempPath = Path.GetTempPath();
            var driveInfo = new DriveInfo(Path.GetPathRoot(tempPath)!);
            long freeMb = driveInfo.AvailableFreeSpace / (1024 * 1024);
            long totalMb = driveInfo.TotalSize / (1024 * 1024);
            builder.AppendLine($"Temp Drive ({driveInfo.Name}): {freeMb:N0} MB free / {totalMb:N0} MB total");
        }
        catch
        {
            // Ignore failure to query drive info
        }

        builder.AppendLine($"Captured mongod logs ({logBuffer.Count} lines):");

        foreach (string line in logBuffer)
        {
            builder.AppendLine($"  {line}");
        }

        builder.AppendLine("=== End of MongoDB diagnostics ===");
        return builder.ToString();
    }

    private sealed class MongoRunnerWrapper(IMongoRunner underlyingMongoRunner) : IMongoRunner
    {
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
            _underlyingMongoRunner = null;
        }
    }
}
