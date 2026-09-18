namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class JaumannStressIntegrator : IObjectiveStressIntegrator
{
    public Matrix3 RotateStress(Matrix3 previous, Matrix3 rotationIncrement) =>
        rotationIncrement * previous * rotationIncrement.Transpose();

    public Matrix3 ObjectiveCorrection(ObjectiveStressUpdateContext c) =>
        c.Spin * c.CauchyStress - c.CauchyStress * c.Spin;
}
