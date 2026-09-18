using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing.Sizing;

public interface IElementSizingField
{
    double SizeAt(Vec3 point);
}
