namespace ImpactLab.Core.Meshing;

public sealed record RefinementRecommendation(
    IReadOnlyList<LocalMeshSizingRegion> Regions,
    double SuggestedBaseCellSizeMeters,
    string Reason
);
