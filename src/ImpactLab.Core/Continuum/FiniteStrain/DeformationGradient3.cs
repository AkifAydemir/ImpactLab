namespace ImpactLab.Core.Continuum.FiniteStrain;

public readonly record struct DeformationGradient3(Matrix3 Value)
{
    public static DeformationGradient3 Identity => new(Matrix3.Identity);
    public double J => Value.Determinant;
    public Matrix3 RightCauchyGreen => Value.Transpose() * Value;
    public Matrix3 LeftCauchyGreen => Value * Value.Transpose();

    public void Validate()
    {
        if (J <= 1e-10)
            throw new InvalidOperationException("Non-positive deformation Jacobian.");
    }
}
