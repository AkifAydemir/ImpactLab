namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainElementState(
    DeformationGradient3 F,
    Matrix3 CauchyStress,
    Matrix3 PlasticDeformationGradient,
    double EquivalentPlasticStrain,
    double TemperatureKelvin,
    double Damage
)
{
    public static FiniteStrainElementState Initial(double temperature = 293.15) =>
        new(DeformationGradient3.Identity, new Matrix3(), Matrix3.Identity, 0, temperature, 0);
}
