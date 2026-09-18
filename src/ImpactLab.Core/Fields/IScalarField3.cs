using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fields;

public interface IScalarField3
{
    double Sample(in Vec3 position, double timeSeconds);
}
