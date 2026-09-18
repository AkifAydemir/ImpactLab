namespace ImpactLab.Core.Results.Storage;

public sealed record ResultStoreCatalogSnapshot(
    int SchemaVersion,
    DateTimeOffset GeneratedUtc,
    IReadOnlyList<ResultStoreRunIndex> Runs
)
{
    public const int CurrentSchema = 1;
}
