namespace ImpactLab.Core.Geometry.Repair;

public sealed record MeshRepairReport(
    int OriginalVertices,
    int OriginalTriangles,
    int FinalVertices,
    int FinalTriangles,
    IReadOnlyList<MeshRepairIssue> Issues,
    bool IsWatertight
);
