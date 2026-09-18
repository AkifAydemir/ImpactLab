namespace ImpactLab.Core.Sparse.Iterative;

public sealed class JacobiPreconditioner : ISparsePreconditioner
{
    private double[] _inv = [];
    public string Id => "jacobi";

    public void Build(SparseCsrMatrix m)
    {
        _inv = new double[m.RowCount];
        for (var r = 0; r < m.RowCount; r++)
        {
            double d = 0;
            for (var k = m.RowPointers[r]; k < m.RowPointers[r + 1]; k++)
                if (m.ColumnIndices[k] == r)
                {
                    d = m.Values[k];
                    break;
                }
            _inv[r] = Math.Abs(d) < 1e-30 ? 1 : 1 / d;
        }
    }

    public void Apply(ReadOnlySpan<double> r, Span<double> z)
    {
        for (var i = 0; i < r.Length; i++)
            z[i] = r[i] * _inv[i];
    }
}
