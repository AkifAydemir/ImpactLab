namespace ImpactLab.Extractor;

public sealed record RepositoryManifest(
    IReadOnlyList<RepositoryManifestEntry> Files,
    long TotalBytes,
    string AggregateSha256
);
