namespace ImpactLab.Core.Results.Derived;

public static class PrincipalStressCalculator
{
    public static PrincipalStressValues Approximate(
        double sxx,
        double syy,
        double szz,
        double sxy,
        double syz,
        double szx
    )
    {
        var mean = (sxx + syy + szz) / 3.0;
        var q = Math.Sqrt(
            (
                (sxx - mean) * (sxx - mean)
                + (syy - mean) * (syy - mean)
                + (szz - mean) * (szz - mean)
                + 2 * (sxy * sxy + syz * syz + szx * szx)
            ) / 6.0
        );
        return new(mean + 2 * q, mean, mean - 2 * q);
    }
}
