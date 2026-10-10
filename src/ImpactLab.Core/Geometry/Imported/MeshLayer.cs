namespace ImpactLab.Core.Geometry.Imported;

public sealed record MeshLayer(
    int Index,
    double MinimumCoordinateMeters,
    double MaximumCoordinateMeters,
    TriangleMeshAsset Mesh
);
