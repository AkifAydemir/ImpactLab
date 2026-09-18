namespace ImpactLab.Core.Materials;

public readonly record struct ConstitutiveCurvePoint(double Strain, double StressPa)
{
    public void Validate()
    {
        if (!double.IsFinite(Strain) || !double.IsFinite(StressPa))
            throw new InvalidOperationException("Constitutive curve point must be finite.");
    }
}
