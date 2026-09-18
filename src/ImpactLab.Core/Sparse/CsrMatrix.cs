namespace ImpactLab.Core.Sparse;

public class CsrMatrix
{
    public CsrMatrix(int rows, int columns, int[] rowOffsets, int[] columnIndices, double[] values)
    {
        Rows = rows;
        Columns = columns;
        RowOffsets = rowOffsets;
        ColumnIndices = columnIndices;
        Values = values;
    }

    public int Rows { get; }
    public int Columns { get; }
    public int[] RowOffsets { get; }
    public int[] ColumnIndices { get; }
    public double[] Values { get; }

    public static CsrMatrix FromTriplets(
        int rows,
        int columns,
        IReadOnlyList<SparseTriplet> triplets
    )
    {
        var rowOffsets = new int[rows + 1];
        foreach (var triplet in triplets)
        {
            rowOffsets[triplet.Row + 1]++;
        }

        for (var row = 0; row < rows; row++)
        {
            rowOffsets[row + 1] += rowOffsets[row];
        }

        var columnIndices = new int[triplets.Count];
        var values = new double[triplets.Count];
        for (var index = 0; index < triplets.Count; index++)
        {
            columnIndices[index] = triplets[index].Column;
            values[index] = triplets[index].Value;
        }

        return new CsrMatrix(rows, columns, rowOffsets, columnIndices, values);
    }

    public void Multiply(ReadOnlySpan<double> x, Span<double> y)
    {
        if (x.Length != Columns || y.Length != Rows)
        {
            throw new ArgumentException("Sparse multiply dimension mismatch.");
        }

        for (var row = 0; row < Rows; row++)
        {
            double sum = 0;
            for (var index = RowOffsets[row]; index < RowOffsets[row + 1]; index++)
            {
                sum += Values[index] * x[ColumnIndices[index]];
            }

            y[row] = sum;
        }
    }

    public double Diagonal(int row)
    {
        for (var index = RowOffsets[row]; index < RowOffsets[row + 1]; index++)
        {
            if (ColumnIndices[index] == row)
            {
                return Values[index];
            }
        }

        return 0;
    }
}
