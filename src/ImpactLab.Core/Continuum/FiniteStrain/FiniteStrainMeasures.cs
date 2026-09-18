namespace ImpactLab.Core.Continuum.FiniteStrain;

public static class FiniteStrainMeasures
{
    public static Matrix3 GreenLagrange(DeformationGradient3 f) =>
        (f.RightCauchyGreen - Matrix3.Identity) * 0.5;

    public static Matrix3 EulerAlmansi(DeformationGradient3 f) =>
        (Matrix3.Identity - f.LeftCauchyGreen.Inverse()) * 0.5;

    public static Matrix3 VelocityGradient(Matrix3 fDot, DeformationGradient3 f) =>
        fDot * f.Value.Inverse();

    public static Matrix3 RateOfDeformation(Matrix3 velocityGradient) =>
        Matrix3Math.Symmetric(velocityGradient);

    public static Matrix3 Spin(Matrix3 velocityGradient) => Matrix3Math.Skew(velocityGradient);
}
