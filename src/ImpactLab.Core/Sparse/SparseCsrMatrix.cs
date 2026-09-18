namespace ImpactLab.Core.Sparse;

public sealed class SparseCsrMatrix : CsrMatrix
{
    public SparseCsrMatrix(
        int rows,
        int columns,
        int[] rowPointers,
        int[] columnIndices,
        double[] values
    )
        : base(rows, columns, rowPointers, columnIndices, values) { }

    public int RowCount => Rows;
    public int ColumnCount => Columns;
    public int[] RowPointers => RowOffsets;

    public static new SparseCsrMatrix FromTriplets(
        int rows,
        int columns,
        IReadOnlyList<SparseTriplet> triplets
    )
    {
        var rowPointers = new int[rows + 1];
        foreach (var triplet in triplets)
        {
            rowPointers[triplet.Row + 1]++;
        }

        for (var row = 0; row < rows; row++)
        {
            rowPointers[row + 1] += rowPointers[row];
        }

        var columnIndices = new int[triplets.Count];
        var values = new double[triplets.Count];
        for (var index = 0; index < triplets.Count; index++)
        {
            columnIndices[index] = triplets[index].Column;
            values[index] = triplets[index].Value;
        }

        return new SparseCsrMatrix(rows, columns, rowPointers, columnIndices, values);
    }
}
