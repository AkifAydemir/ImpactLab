namespace ImpactLab.Extractor;

public sealed record ExtractionReport(
    int SourceBlocks,
    int UniqueFiles,
    string OutputRoot,
    IReadOnlyList<string> Paths,
    RepositoryManifest RepositoryManifest
);
