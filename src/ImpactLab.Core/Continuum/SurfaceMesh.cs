namespace ImpactLab.Core.Continuum;

public sealed record SurfaceMesh(TetrahedralMesh Volume, IReadOnlyList<SurfaceTriangle> Triangles);
