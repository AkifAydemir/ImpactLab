namespace ImpactLab.Core.Results.Queries;

public sealed record SpatialFieldQueryResult(
    string FieldId,
    int FrameIndex,
    int Count,
    double Minimum,
    double Maximum,
    double Mean,
    double Rms,
    IReadOnlyList<int> Indices
);
