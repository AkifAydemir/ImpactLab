namespace ImpactLab.Core.Meshing;

public sealed record AdaptiveMeshPlan(
    IReadOnlyList<AdaptiveCell> Leaves,
    int MaximumDepth,
    double MinimumCellSize,
    double MaximumCellSize,
    IReadOnlyDictionary<string, int> RefinementReasons
)
{
    public int LeafCount => Leaves.Count;
}
