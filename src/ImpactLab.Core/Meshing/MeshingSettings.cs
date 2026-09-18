using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed record MeshingSettings
{
    public MeshingMode Mode { get; init; } = MeshingMode.UniformStructured;
    public double BaseCellSizeMeters { get; init; } = 0.01;
    public double MinimumCellSizeMeters { get; init; } = 0.0025;
    public int MaximumRefinementDepth { get; init; } = 3;
    public double BoundaryRefinementBandMeters { get; init; } = 0.015;
    public IReadOnlyList<AdaptiveRefinementRegion> FocusRegions { get; init; } = [];

    public void Validate()
    {
        if (BaseCellSizeMeters <= 0 || MinimumCellSizeMeters <= 0)
            throw new InvalidOperationException("Cell sizes must be positive.");
        if (MinimumCellSizeMeters > BaseCellSizeMeters)
            throw new InvalidOperationException("Minimum cell size cannot exceed base cell size.");
        if (MaximumRefinementDepth < 0 || MaximumRefinementDepth > 8)
            throw new InvalidOperationException(
                "Maximum refinement depth must be between 0 and 8."
            );
    }

    public static MeshingSettings FromCellSize(double cellSize) =>
        new()
        {
            BaseCellSizeMeters = cellSize,
            MinimumCellSizeMeters = Math.Max(cellSize / 4.0, cellSize * 0.125),
        };
}
