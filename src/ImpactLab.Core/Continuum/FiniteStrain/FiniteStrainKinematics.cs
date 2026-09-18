using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainKinematics(
    DeformationGradient3 F,
    Matrix3 GreenLagrange,
    Matrix3 Almansi,
    Matrix3 RateOfDeformation,
    Matrix3 Spin,
    double Jacobian
)
{
    public static FiniteStrainKinematics FromGradient(Matrix3 f, Matrix3 fDot)
    {
        var F = new DeformationGradient3(f);
        F.Validate();
        var l = FiniteStrainMeasures.VelocityGradient(fDot, F);
        return new(
            F,
            FiniteStrainMeasures.GreenLagrange(F),
            FiniteStrainMeasures.EulerAlmansi(F),
            FiniteStrainMeasures.RateOfDeformation(l),
            FiniteStrainMeasures.Spin(l),
            F.J
        );
    }
}
