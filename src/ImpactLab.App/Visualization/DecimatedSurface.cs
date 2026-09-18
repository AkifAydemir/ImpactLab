using ImpactLab.Core.Continuum;

namespace ImpactLab.App.Visualization;

public sealed record DecimatedSurface(
    IReadOnlyList<SurfaceTriangle> Triangles,
    int OriginalTriangles,
    double RetainedFraction
);
