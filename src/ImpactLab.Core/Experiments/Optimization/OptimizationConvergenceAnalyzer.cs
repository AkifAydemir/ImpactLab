namespace ImpactLab.Core.Experiments.Optimization;

public static class OptimizationConvergenceAnalyzer
{
    public static IReadOnlyList<OptimizationConvergencePoint> Analyze(
        IReadOnlyList<double> objective,
        IReadOnlyList<bool> feasible
    )
    {
        var r = new List<OptimizationConvergencePoint>();
        var best = double.PositiveInfinity;
        for (var i = 0; i < objective.Count; i++)
        {
            var old = best;
            if (i < feasible.Count && feasible[i])
                best = Math.Min(best, objective[i]);
            var f = feasible.Take(i + 1).Count(x => x) / (double)(i + 1);
            r.Add(
                new(
                    i + 1,
                    best,
                    double.IsInfinity(old) || double.IsInfinity(best) ? 0 : old - best,
                    f
                )
            );
        }
        return r;
    }
}
