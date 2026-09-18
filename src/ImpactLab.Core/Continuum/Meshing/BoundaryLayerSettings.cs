namespace ImpactLab.Core.Continuum.Meshing;

public sealed record BoundaryLayerSettings(
    bool Enabled = false,
    int Layers = 3,
    double FirstLayerThicknessMeters = 0.001,
    double GrowthRate = 1.25,
    IReadOnlyList<string>? PartIds = null
);
