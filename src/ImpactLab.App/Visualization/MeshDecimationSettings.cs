namespace ImpactLab.App.Visualization;

public sealed record MeshDecimationSettings(
    int TargetTriangles = 250000,
    double FeatureAngleDegrees = 35,
    bool PreserveBoundaries = true,
    bool PreservePartInterfaces = true
);
