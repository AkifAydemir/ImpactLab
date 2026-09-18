namespace ImpactLab.Core.Sparse.Iterative;

public static class VectorOps
{
    public static double Dot(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
    {
        double s = 0;
        for (var i = 0; i < a.Length; i++)
            s += a[i] * b[i];
        return s;
    }

    public static double Norm(ReadOnlySpan<double> a) => Math.Sqrt(Dot(a, a));

    public static void Axpy(double alpha, ReadOnlySpan<double> x, Span<double> y)
    {
        for (var i = 0; i < x.Length; i++)
            y[i] += alpha * x[i];
    }
}
