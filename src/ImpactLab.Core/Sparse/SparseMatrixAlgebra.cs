namespace ImpactLab.Core.Sparse;

public static class SparseMatrixAlgebra
{
    public static SparseCsrMatrix Combine(params (SparseCsrMatrix M, double Scale)[] terms)
    {
        if (terms.Length == 0)
            throw new ArgumentException("At least one term required.");
        var trip = new SparseTripletBuilder(terms[0].M.RowCount, terms[0].M.ColumnCount);
        foreach (var (m, s) in terms)
            for (var r = 0; r < m.RowCount; r++)
            for (var k = m.RowPointers[r]; k < m.RowPointers[r + 1]; k++)
                trip.Add(r, m.ColumnIndices[k], m.Values[k] * s);
        return trip.BuildCsr();
    }

    public static double[] Multiply(SparseCsrMatrix m, ReadOnlySpan<double> x)
    {
        var y = new double[m.RowCount];
        for (var r = 0; r < m.RowCount; r++)
        {
            double s = 0;
            for (var k = m.RowPointers[r]; k < m.RowPointers[r + 1]; k++)
                s += m.Values[k] * x[m.ColumnIndices[k]];
            y[r] = s;
        }
        return y;
    }
}
