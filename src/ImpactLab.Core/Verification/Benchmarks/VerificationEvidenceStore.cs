using System.Globalization;
using System.Text.Json;

namespace ImpactLab.Core.Verification.Benchmarks;

public sealed class VerificationEvidenceStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _root;

    public VerificationEvidenceStore(string root)
    {
        _root = root;
        Directory.CreateDirectory(root);
    }

    public string Save(VerificationBenchmarkSuiteResult result)
    {
        var stamp = result.ExecutedUtc.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var dir = Path.Combine(_root, stamp);
        Directory.CreateDirectory(dir);
        var records = result
            .Benchmarks.Select(x => new VerificationEvidenceRecord(
                x.Id,
                x.Passed,
                result.ExecutedUtc,
                Environment.Version.ToString(),
                Environment.MachineName,
                x.Metrics,
                x.Evidence,
                x.Error
            ))
            .ToArray();
        var path = Path.Combine(dir, "verification-evidence.json");
        File.WriteAllText(path, JsonSerializer.Serialize(records, Options));
        return path;
    }

    public IReadOnlyList<VerificationEvidenceRecord> Load(string path) =>
        JsonSerializer.Deserialize<VerificationEvidenceRecord[]>(File.ReadAllText(path), Options)
        ?? [];
}
