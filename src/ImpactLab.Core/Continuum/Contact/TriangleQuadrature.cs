namespace ImpactLab.Core.Continuum.Contact;

public static class TriangleQuadrature
{
    public static IReadOnlyList<SurfaceQuadraturePoint> OnePoint { get; } =
    [new(1.0 / 3, 1.0 / 3, 0.5)];
    public static IReadOnlyList<SurfaceQuadraturePoint> ThreePoint { get; } =
    [
        new(1.0 / 6, 1.0 / 6, 1.0 / 6),
        new(2.0 / 3, 1.0 / 6, 1.0 / 6),
        new(1.0 / 6, 2.0 / 3, 1.0 / 6),
    ];
}
