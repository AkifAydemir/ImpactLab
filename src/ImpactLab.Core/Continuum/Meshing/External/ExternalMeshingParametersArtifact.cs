namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalMeshingParametersArtifact(
    double TargetSizeMeters,
    double MinimumSizeMeters,
    double MaximumSizeMeters,
    double CurvatureFactor,
    double GrowthRate,
    bool OptimizeSlivers,
    bool PreserveBoundary,
    BoundaryLayerSettings BoundaryLayer
);
