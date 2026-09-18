using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing.Sizing;

public sealed record CurvatureSizingField(double MinSize, double MaxSize, double CurvatureScale)
    : IElementSizingField
{
    public double SizeAt(Vec3 point) =>
        Math.Clamp(MaxSize / (1 + CurvatureScale * point.Length), MinSize, MaxSize);
}
