namespace ImpactLab.Core.Materials;

public sealed record MaterialPropertyBand(
    double Nominal,
    double Minimum,
    double Maximum,
    string Unit
)
{
    public void Validate()
    {
        if (!double.IsFinite(Nominal) || !double.IsFinite(Minimum) || !double.IsFinite(Maximum))
            throw new InvalidOperationException("Property band values must be finite.");
        if (Minimum > Nominal || Nominal > Maximum)
            throw new InvalidOperationException(
                "Property band must satisfy minimum <= nominal <= maximum."
            );
    }

    public double RelativeHalfWidth =>
        Math.Abs(Nominal) <= 1e-18 ? 0.0 : (Maximum - Minimum) / (2.0 * Math.Abs(Nominal));
}
