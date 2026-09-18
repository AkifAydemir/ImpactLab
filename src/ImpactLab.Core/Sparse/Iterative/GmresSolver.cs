namespace ImpactLab.Core.Sparse.Iterative;

public sealed class GmresSolver
{
    public (double[] Solution, IterativeSolveMetrics Metrics) Solve(
        SparseCsrMatrix A,
        ReadOnlySpan<double> b,
        ISparsePreconditioner M,
        IterativeSolverSettings? settings = null,
        CancellationToken ct = default
    )
    {
        var s = settings ?? new();
        var x = new double[b.Length];
        var r = b.ToArray();
        M.Build(A);
        var r0 = VectorOps.Norm(r);
        var hist = new List<double> { r0 };
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var outer = 0; outer < s.MaxIterations; outer += Math.Max(1, s.Restart))
        {
            ct.ThrowIfCancellationRequested();
            var z = new double[r.Length];
            M.Apply(r, z);
            var Az = SparseMatrixAlgebra.Multiply(A, z);
            var den = Math.Max(VectorOps.Dot(Az, Az), 1e-30);
            var alpha = VectorOps.Dot(r, Az) / den;
            for (var i = 0; i < x.Length; i++)
            {
                x[i] += alpha * z[i];
                r[i] -= alpha * Az[i];
            }
            var rn = VectorOps.Norm(r);
            hist.Add(rn);
            if (rn <= Math.Max(s.AbsoluteTolerance, s.RelativeTolerance * r0))
            {
                sw.Stop();
                return (x, new(true, outer + 1, r0, rn, sw.Elapsed, hist));
            }
        }
        sw.Stop();
        return (x, new(false, s.MaxIterations, r0, VectorOps.Norm(r), sw.Elapsed, hist));
    }
}
