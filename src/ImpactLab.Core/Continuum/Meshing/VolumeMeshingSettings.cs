namespace ImpactLab.Core.Continuum.Meshing;

public sealed record VolumeMeshingSettings(
    double TargetSizeMeters = 0.01,
    double MinimumSizeMeters = 0.002,
    double MaximumSizeMeters = 0.05,
    double CurvatureFactor = 0.5,
    double GrowthRate = 1.4,
    bool OptimizeSlivers = true,
    bool PreserveBoundary = true,
    BoundaryLayerSettings? BoundaryLayer = null
)
{
    public void Validate()
    {
        if (
            TargetSizeMeters <= 0
            || MinimumSizeMeters <= 0
            || MaximumSizeMeters < MinimumSizeMeters
            || GrowthRate < 1
        )
            throw new InvalidOperationException("Invalid volume meshing settings.");
    }
}
