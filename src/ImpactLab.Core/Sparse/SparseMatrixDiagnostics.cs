namespace ImpactLab.Core.Sparse;

public static class SparseMatrixDiagnostics
{
    public static (int NonZeros, double Density, double MinDiagonal, double MaxDiagonal) Analyze(
        CsrMatrix a
    )
    {
        var nnz = a.Values.Length;
        var min = double.PositiveInfinity;
        var max = double.NegativeInfinity;
        for (var i = 0; i < a.Rows; i++)
        {
            var d = a.Diagonal(i);
            min = Math.Min(min, d);
            max = Math.Max(max, d);
        }
        return (nnz, (double)nnz / Math.Max(1L, (long)a.Rows * a.Columns), min, max);
    }
}
