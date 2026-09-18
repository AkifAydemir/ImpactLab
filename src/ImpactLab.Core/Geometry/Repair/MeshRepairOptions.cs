namespace ImpactLab.Core.Geometry.Repair;

public sealed record MeshRepairOptions(
    double WeldToleranceMeters = 1e-7,
    double DegenerateAreaTolerance = 1e-14,
    bool RemoveDegenerate = true,
    bool WeldVertices = true,
    bool OrientNormals = true,
    bool PatchSmallHoles = true,
    int MaxHoleEdges = 32
);
