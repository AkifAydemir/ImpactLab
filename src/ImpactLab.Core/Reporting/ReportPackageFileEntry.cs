namespace ImpactLab.Core.Reporting;

public sealed record ReportPackageFileEntry(
    string Path,
    string MediaType,
    long Bytes,
    string Sha256
);
