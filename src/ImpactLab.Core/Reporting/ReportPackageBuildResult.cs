namespace ImpactLab.Core.Reporting;

public sealed record ReportPackageBuildResult(
    string RootDirectory,
    DeterministicReportPackageManifest Manifest,
    string ManifestPath
);
