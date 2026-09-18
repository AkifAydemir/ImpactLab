namespace ImpactLab.Core.Sparse.Iterative;

public sealed class PcgSolver
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
        var z = new double[b.Length];
        M.Build(A);
        M.Apply(r, z);
        var p = z.ToArray();
        var rz = VectorOps.Dot(r, z);
        var r0 = VectorOps.Norm(r);
        var hist = new List<double> { r0 };
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var k = 0;
        for (; k < s.MaxIterations; k++)
        {
            ct.ThrowIfCancellationRequested();
            var Ap = SparseMatrixAlgebra.Multiply(A, p);
            var den = VectorOps.Dot(p, Ap);
            if (Math.Abs(den) < 1e-30)
                break;
            var alpha = rz / den;
            for (var i = 0; i < x.Length; i++)
            {
                x[i] += alpha * p[i];
                r[i] -= alpha * Ap[i];
            }
            var rn = VectorOps.Norm(r);
            hist.Add(rn);
            if (rn <= Math.Max(s.AbsoluteTolerance, s.RelativeTolerance * r0))
            {
                sw.Stop();
                return (x, new(true, k + 1, r0, rn, sw.Elapsed, hist));
            }
            M.Apply(r, z);
            var rz2 = VectorOps.Dot(r, z);
            var beta = rz2 / Math.Max(rz, 1e-30);
            for (var i = 0; i < p.Length; i++)
                p[i] = z[i] + beta * p[i];
            rz = rz2;
        }
        sw.Stop();
        return (x, new(false, k, r0, VectorOps.Norm(r), sw.Elapsed, hist));
    }
}
