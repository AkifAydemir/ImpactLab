namespace ImpactLab.Core.Materials;

public sealed record MaterialProvenance(
    string SourceName,
    string? SourceUri = null,
    string? Specification = null,
    string? BatchOrLot = null,
    DateTimeOffset? TestedAt = null,
    string Notes = ""
)
{
    public bool IsTraceable =>
        !string.IsNullOrWhiteSpace(SourceName)
        && (
            !string.IsNullOrWhiteSpace(SourceUri)
            || !string.IsNullOrWhiteSpace(Specification)
            || !string.IsNullOrWhiteSpace(BatchOrLot)
        );
}
