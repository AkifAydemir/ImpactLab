using ImpactLab.Core.Extensions;

namespace ImpactLab.App.Services;

public sealed record ExtensionLoadFailure(string Path, string Message);

public sealed record ExtensionDiscoveryResult(
    IReadOnlyList<ExtensionPackage> Packages,
    IReadOnlyList<ExtensionLoadFailure> Failures
);

public sealed class ExtensionDiscoveryService
{
    public ExtensionDiscoveryResult Discover(string directory)
    {
        var scan = ExtensionPackageScanner.ScanDetailed(directory);
        return new(
            scan.Packages,
            scan.Failures.Select(x => new ExtensionLoadFailure(x.ManifestPath, x.Message)).ToArray()
        );
    }
}
