namespace ImpactLab.Core.Geometry.Imported;

public sealed record MeshRepairReport(
    int VertexCount,
    int TriangleCount,
    int DegenerateTriangles,
    int DuplicateTriangles,
    int BoundaryEdges,
    bool IsLikelyClosed,
    IReadOnlyList<string> Warnings
);
