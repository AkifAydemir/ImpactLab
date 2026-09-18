namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed class NonlinearConstraintProjector
{
    private readonly IReadOnlyList<int> _fixed;

    public NonlinearConstraintProjector(IEnumerable<int> fixedDofs) =>
        _fixed = fixedDofs.Distinct().Order().ToArray();

    public void ApplyToResidual(Span<double> r)
    {
        foreach (var d in _fixed)
            if ((uint)d < (uint)r.Length)
                r[d] = 0;
    }

    public void ApplyToIncrement(Span<double> du)
    {
        foreach (var d in _fixed)
            if ((uint)d < (uint)du.Length)
                du[d] = 0;
    }

    public IReadOnlyList<int> FixedDofs => _fixed;
}
