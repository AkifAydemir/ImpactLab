namespace ImpactLab.Core.Continuum.FiniteStrain;

public static class JacobianQualityMonitor
{
    public static IReadOnlyList<int> Invalid(
        IReadOnlyList<FiniteStrainElementState> s,
        double minJ = 0.05
    ) => s.Select((x, i) => (x, i)).Where(z => z.x.F.J < minJ).Select(z => z.i).ToArray();
}
