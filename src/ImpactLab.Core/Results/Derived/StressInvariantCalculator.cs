namespace ImpactLab.Core.Results.Derived;

public static class StressInvariantCalculator
{
    public static double VonMises(
        double sxx,
        double syy,
        double szz,
        double sxy,
        double syz,
        double szx
    ) =>
        Math.Sqrt(
            0.5
                * (
                    (sxx - syy) * (sxx - syy)
                    + (syy - szz) * (syy - szz)
                    + (szz - sxx) * (szz - sxx)
                )
                + 3 * (sxy * sxy + syz * syz + szx * szx)
        );

    public static double Mean(double sxx, double syy, double szz) => (sxx + syy + szz) / 3.0;
}
