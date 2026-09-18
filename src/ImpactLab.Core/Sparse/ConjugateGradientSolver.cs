namespace ImpactLab.Core.Sparse;

public sealed class ConjugateGradientSolver : ISparseLinearSolver
{
    public ConjugateGradientSolver(double tolerance = 1e-9, int maxIterations = 10000)
    {
        Tolerance = tolerance;
        MaxIterations = maxIterations;
    }

    public string Id => "managed-cg";
    public bool IsAvailable => true;
    public double Tolerance { get; }
    public int MaxIterations { get; }

    public LinearSolveReport Solve(
        CsrMatrix a,
        ReadOnlySpan<double> b,
        Span<double> x,
        CancellationToken ct = default
    )
    {
        var n = b.Length;
        var r = new double[n];
        var z = new double[n];
        var p = new double[n];
        var ap = new double[n];
        a.Multiply(x, ap);
        for (var i = 0; i < n; i++)
            r[i] = b[i] - ap[i];
        var m = new JacobiPreconditioner(a);
        m.Apply(r, z);
        Array.Copy(z, p, n);
        var rz = Dot(r, z);
        var initial = Math.Sqrt(Math.Max(Dot(r, r), 1e-300));
        if (initial < Tolerance)
            return new(true, 0, initial, Id);
        for (var it = 1; it <= MaxIterations; it++)
        {
            ct.ThrowIfCancellationRequested();
            a.Multiply(p, ap);
            var den = Dot(p, ap);
            if (Math.Abs(den) < 1e-300)
                return new(false, it, Math.Sqrt(Dot(r, r)), Id);
            var alpha = rz / den;
            for (var i = 0; i < n; i++)
            {
                x[i] += alpha * p[i];
                r[i] -= alpha * ap[i];
            }
            var norm = Math.Sqrt(Dot(r, r));
            if (norm <= Tolerance * Math.Max(1.0, initial))
                return new(true, it, norm, Id);
            m.Apply(r, z);
            var rzNew = Dot(r, z);
            var beta = rzNew / rz;
            for (var i = 0; i < n; i++)
                p[i] = z[i] + beta * p[i];
            rz = rzNew;
        }
        return new(false, MaxIterations, Math.Sqrt(Dot(r, r)), Id);
    }

    private static double Dot(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
    {
        double s = 0;
        for (var i = 0; i < a.Length; i++)
            s += a[i] * b[i];
        return s;
    }
}
