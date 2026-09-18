namespace ImpactLab.Core.Sparse;

public sealed class BiCgStabSolver : ISparseLinearSolver
{
    public BiCgStabSolver(double tolerance = 1e-9, int maxIterations = 20000)
    {
        Tolerance = tolerance;
        MaxIterations = maxIterations;
    }

    public string Id => "managed-bicgstab";
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
        var r0 = new double[n];
        var p = new double[n];
        var v = new double[n];
        var s = new double[n];
        var t = new double[n];
        a.Multiply(x, v);
        for (var i = 0; i < n; i++)
            r0[i] = r[i] = b[i] - v[i];
        double rho = 1,
            alpha = 1,
            omega = 1;
        Array.Clear(v);
        Array.Clear(p);
        for (var it = 1; it <= MaxIterations; it++)
        {
            ct.ThrowIfCancellationRequested();
            var rn = Dot(r0, r);
            if (Math.Abs(rn) < 1e-300)
                break;
            var beta = (rn / rho) * (alpha / omega);
            for (var i = 0; i < n; i++)
                p[i] = r[i] + beta * (p[i] - omega * v[i]);
            a.Multiply(p, v);
            alpha = rn / Math.Max(Dot(r0, v), 1e-300);
            for (var i = 0; i < n; i++)
                s[i] = r[i] - alpha * v[i];
            if (Norm(s) < Tolerance)
            {
                for (var i = 0; i < n; i++)
                    x[i] += alpha * p[i];
                return new(true, it, Norm(s), Id);
            }
            a.Multiply(s, t);
            omega = Dot(t, s) / Math.Max(Dot(t, t), 1e-300);
            for (var i = 0; i < n; i++)
            {
                x[i] += alpha * p[i] + omega * s[i];
                r[i] = s[i] - omega * t[i];
            }
            var norm = Norm(r);
            if (norm < Tolerance)
                return new(true, it, norm, Id);
            rho = rn;
            if (Math.Abs(omega) < 1e-300)
                break;
        }
        return new(false, MaxIterations, Norm(r), Id);
    }

    private static double Dot(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
    {
        double s = 0;
        for (var i = 0; i < a.Length; i++)
            s += a[i] * b[i];
        return s;
    }

    private static double Norm(ReadOnlySpan<double> x) => Math.Sqrt(Dot(x, x));
}
