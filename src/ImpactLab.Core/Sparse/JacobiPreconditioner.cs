namespace ImpactLab.Core.Sparse;

public sealed class JacobiPreconditioner : IPreconditioner
{
    private readonly double[] _inverse;

    public JacobiPreconditioner(CsrMatrix a)
    {
        _inverse = new double[a.Rows];
        for (var i = 0; i < a.Rows; i++)
        {
            var d = a.Diagonal(i);
            _inverse[i] = Math.Abs(d) < 1e-30 ? 1.0 : 1.0 / d;
        }
    }

    public string Id => "jacobi";

    public void Apply(ReadOnlySpan<double> input, Span<double> output)
    {
        for (var i = 0; i < input.Length; i++)
            output[i] = input[i] * _inverse[i];
    }
}
