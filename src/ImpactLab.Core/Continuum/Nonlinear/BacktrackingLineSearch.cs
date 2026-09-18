namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed class BacktrackingLineSearch
{
    private readonly LineSearchSettings _s;

    public BacktrackingLineSearch(LineSearchSettings s) => _s = s;

    public (double Scale, double Norm) Search(double baseNorm, Func<double, double> evaluate)
    {
        if (!_s.Enabled)
            return (1, evaluate(1));
        var scale = _s.InitialScale;
        var best = (Scale: scale, Norm: evaluate(scale));
        for (var i = 1; i < _s.MaxTrials && best.Norm >= baseNorm && scale > _s.MinimumScale; i++)
        {
            scale = Math.Max(_s.MinimumScale, scale * _s.Reduction);
            var n = evaluate(scale);
            if (n < best.Norm)
                best = (scale, n);
        }
        return best;
    }
}
