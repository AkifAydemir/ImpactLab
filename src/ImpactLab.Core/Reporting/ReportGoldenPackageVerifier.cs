using System.Text.Json;

namespace ImpactLab.Core.Reporting;

public static class ReportGoldenPackageVerifier
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    );

    public static ReportGoldenVerificationResult Verify(string directory)
    {
        var findings = new List<string>();
        var manifestPath = Path.Combine(directory, "manifest.json");
        if (!File.Exists(manifestPath))
            return new(false, ["manifest.json is missing."]);
        DeterministicReportPackageManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<DeterministicReportPackageManifest>(
                File.ReadAllText(manifestPath),
                SerializerOptions
            );
        }
        catch (Exception ex)
        {
            return new(false, [$"manifest.json could not be parsed: {ex.Message}"]);
        }
        if (manifest is null)
            return new(false, ["manifest.json deserialized to null."]);
        if (manifest.SchemaVersion != 1)
            findings.Add($"Unsupported report package schema {manifest.SchemaVersion}.");
        foreach (var entry in manifest.Files)
        {
            if (
                entry.Path.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(entry.Path)
            )
            {
                findings.Add($"Unsafe package path: {entry.Path}");
                continue;
            }
            var path = Path.Combine(
                directory,
                entry.Path.Replace('/', Path.DirectorySeparatorChar)
            );
            if (!File.Exists(path))
            {
                findings.Add($"Missing package file: {entry.Path}");
                continue;
            }
            var info = new FileInfo(path);
            if (info.Length != entry.Bytes)
                findings.Add(
                    $"Byte count mismatch for {entry.Path}: {info.Length} != {entry.Bytes}"
                );
            var hash = ReportPackageHash.File(path);
            if (!hash.Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
                findings.Add($"SHA-256 mismatch for {entry.Path}.");
        }
        return new(findings.Count == 0, findings);
    }
}
