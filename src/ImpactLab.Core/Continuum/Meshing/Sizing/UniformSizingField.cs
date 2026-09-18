using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing.Sizing;

public sealed record UniformSizingField(double SizeMeters) : IElementSizingField
{
    public double SizeAt(Vec3 point) => SizeMeters;
}
