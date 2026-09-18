using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fields;

public sealed record ConstantScalarField(double Value) : IScalarField3
{
    public double Sample(in Vec3 position, double timeSeconds) => Value;
}
