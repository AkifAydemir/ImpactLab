namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class GreenNaghdiStressIntegrator : IObjectiveStressIntegrator
{
    public Matrix3 RotateStress(Matrix3 previous, Matrix3 rotationIncrement) =>
        rotationIncrement * previous * rotationIncrement.Transpose();

    public Matrix3 ObjectiveCorrection(ObjectiveStressUpdateContext c) =>
        Matrix3Math.Skew(c.RotationIncrement) * c.CauchyStress
        - c.CauchyStress * Matrix3Math.Skew(c.RotationIncrement);
}
