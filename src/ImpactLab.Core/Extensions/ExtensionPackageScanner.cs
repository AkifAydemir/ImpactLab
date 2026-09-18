using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImpactLab.Core.Extensions;

public sealed record ExtensionPackage(
    string Directory,
    string ManifestPath,
    ExtensionPackageManifest Manifest,
    string EntryAssemblyPath
);

public sealed record ExtensionPackageScanFailure(string ManifestPath, string Message);

public sealed record ExtensionPackageScanResult(
    IReadOnlyList<ExtensionPackage> Packages,
    IReadOnlyList<ExtensionPackageScanFailure> Failures
);

public static class ExtensionPackageScanner
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static IReadOnlyList<ExtensionPackage> Scan(string root) => ScanDetailed(root).Packages;

    public static ExtensionPackageScanResult ScanDetailed(string root)
    {
        if (!Directory.Exists(root))
            return new([], []);
        var packages = new List<ExtensionPackage>();
        var failures = new List<ExtensionPackageScanFailure>();
        foreach (
            var manifestPath in Directory
                .EnumerateFiles(root, "impactlab.extension.json", SearchOption.AllDirectories)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
        )
        {
            try
            {
                var m =
                    JsonSerializer.Deserialize<ExtensionPackageManifest>(
                        File.ReadAllText(manifestPath),
                        Json
                    ) ?? throw new InvalidDataException("Empty extension manifest.");
                if (string.IsNullOrWhiteSpace(m.Id) || string.IsNullOrWhiteSpace(m.EntryAssembly))
                    throw new InvalidDataException("Extension id and entryAssembly are required.");
                var dir = Path.GetFullPath(Path.GetDirectoryName(manifestPath)!);
                var entry = Path.GetFullPath(Path.Combine(dir, m.EntryAssembly));
                if (
                    !entry.StartsWith(
                        dir + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    throw new InvalidDataException("Entry assembly escapes package directory.");
                if (!File.Exists(entry))
                    throw new FileNotFoundException("Extension entry assembly not found.", entry);
                packages.Add(new(dir, Path.GetFullPath(manifestPath), m, entry));
            }
            catch (Exception ex)
            {
                failures.Add(new(Path.GetFullPath(manifestPath), ex.Message));
            }
        }
        return new(packages, failures);
    }
}
