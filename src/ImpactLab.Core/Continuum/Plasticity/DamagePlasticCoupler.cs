namespace ImpactLab.Core.Continuum.Plasticity;

public static class DamagePlasticCoupler
{
    public static double UpdateDamage(
        double current,
        double eqPlastic,
        double threshold,
        double failure
    )
    {
        if (eqPlastic <= threshold)
            return current;
        var x = (eqPlastic - threshold) / Math.Max(failure - threshold, 1e-12);
        return Math.Clamp(Math.Max(current, x), 0, 0.9999);
    }

    public static double StiffnessScale(double damage, double residual = 1e-4) =>
        Math.Max(residual, 1 - damage);
}
