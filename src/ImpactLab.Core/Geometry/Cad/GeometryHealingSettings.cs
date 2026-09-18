namespace ImpactLab.Core.Geometry.Cad;

public sealed record GeometryHealingSettings(
    double StitchToleranceMeters = 1e-6,
    double RemoveEdgeBelowMeters = 1e-7,
    double RemoveFaceBelowM2 = 1e-10,
    bool FixOrientation = true,
    bool MergeCoplanarFaces = true,
    bool CloseSmallGaps = true
);
