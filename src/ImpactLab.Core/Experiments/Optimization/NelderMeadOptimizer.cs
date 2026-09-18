namespace ImpactLab.Core.Experiments.Optimization;

public sealed class NelderMeadOptimizer : IOptimizer
{
    public string Id => "nelder-mead";

    public async Task<IReadOnlyList<OptimizationEvaluation>> OptimizeAsync(
        IReadOnlyList<OptimizationVariable> v,
        Func<
            IReadOnlyDictionary<string, double>,
            CancellationToken,
            Task<(double Objective, bool Feasible, string? Artifact)>
        > eval,
        int maxEvaluations,
        CancellationToken ct = default
    )
    {
        var trace = new List<OptimizationEvaluation>();
        var simplex = new List<double[]> { v.Select(x => x.Initial).ToArray() };
        for (var i = 0; i < v.Count; i++)
        {
            var x = v.Select(q => q.Initial).ToArray();
            x[i] = Clamp(v[i], x[i] + 0.05 * Math.Max(Math.Abs(x[i]), v[i].Scale));
            simplex.Add(x);
        }
        var scored = new List<(double[] X, double F, bool Ok, string? A)>();
        foreach (var x in simplex)
        {
            var r = await eval(Map(v, x), ct);
            scored.Add((x, r.Objective, r.Feasible, r.Artifact));
            trace.Add(new(trace.Count, Map(v, x), r.Objective, r.Feasible, r.Artifact));
        }
        while (trace.Count < maxEvaluations)
        {
            scored = scored.OrderBy(x => x.Ok ? x.F : double.PositiveInfinity).ToList();
            var centroid = new double[v.Count];
            for (var i = 0; i < v.Count; i++)
            for (var j = 0; j < v.Count; j++)
                centroid[j] += scored[i].X[j] / v.Count;
            var worst = scored[^1].X;
            var xr = centroid.Select((c, j) => Clamp(v[j], c + (c - worst[j]))).ToArray();
            var rr = await eval(Map(v, xr), ct);
            trace.Add(new(trace.Count, Map(v, xr), rr.Objective, rr.Feasible, rr.Artifact));
            scored[^1] = (xr, rr.Objective, rr.Feasible, rr.Artifact);
            if (trace.Count >= maxEvaluations)
                break;
        }
        return trace;
    }

    private static double Clamp(OptimizationVariable v, double x) =>
        Math.Clamp(x, v.Minimum, v.Maximum);

    private static IReadOnlyDictionary<string, double> Map(
        IReadOnlyList<OptimizationVariable> v,
        double[] x
    ) => v.Select((q, i) => (q.Id, x[i])).ToDictionary(z => z.Id, z => z.Item2);
}
