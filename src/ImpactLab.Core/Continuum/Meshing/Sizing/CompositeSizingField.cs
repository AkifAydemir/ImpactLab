using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing.Sizing;

public sealed class CompositeSizingField : IElementSizingField
{
    private readonly IReadOnlyList<IElementSizingField> _f;

    public CompositeSizingField(params IElementSizingField[] f) => _f = f;

    public double SizeAt(Vec3 p) => _f.Count == 0 ? 0.01 : _f.Min(x => x.SizeAt(p));
}
