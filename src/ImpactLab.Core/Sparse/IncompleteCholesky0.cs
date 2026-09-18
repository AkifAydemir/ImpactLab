namespace ImpactLab.Core.Sparse;

public sealed class IncompleteCholesky0 : IPreconditioner
{
    private readonly double[] _diag;

    public IncompleteCholesky0(CsrMatrix a)
    {
        _diag = new double[a.Rows];
        for (var i = 0; i < a.Rows; i++)
            _diag[i] = 1 / Math.Sqrt(Math.Max(Math.Abs(a.Diagonal(i)), 1e-30));
    }

    public string Id => "ichol0-diagonal-safe";

    public void Apply(ReadOnlySpan<double> x, Span<double> y)
    {
        for (var i = 0; i < x.Length; i++)
            y[i] = x[i] * _diag[i] * _diag[i];
    }
}
