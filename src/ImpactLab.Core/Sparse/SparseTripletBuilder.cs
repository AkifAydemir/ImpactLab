namespace ImpactLab.Core.Sparse;

public sealed class SparseTripletBuilder
{
    private readonly List<SparseTriplet> _items = [];
    private readonly int? _rows;
    private readonly int? _columns;

    public SparseTripletBuilder() { }

    public SparseTripletBuilder(int rows, int columns)
    {
        if (rows < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows));
        }

        if (columns < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns));
        }

        _rows = rows;
        _columns = columns;
    }

    public void Add(int row, int column, double value)
    {
        if (Math.Abs(value) > 0)
        {
            _items.Add(new SparseTriplet(row, column, value));
        }
    }

    public CsrMatrix Build(int rows, int columns) => CsrMatrix.FromTriplets(rows, columns, Merge());

    public SparseCsrMatrix BuildCsr()
    {
        if (_rows is null || _columns is null)
        {
            throw new InvalidOperationException(
                "Matrix dimensions were not supplied to the builder."
            );
        }

        return SparseCsrMatrix.FromTriplets(_rows.Value, _columns.Value, Merge());
    }

    private SparseTriplet[] Merge() =>
        _items
            .GroupBy(item => (item.Row, item.Column))
            .Select(group => new SparseTriplet(
                group.Key.Row,
                group.Key.Column,
                group.Sum(item => item.Value)
            ))
            .Where(item => Math.Abs(item.Value) > 1e-30)
            .OrderBy(item => item.Row)
            .ThenBy(item => item.Column)
            .ToArray();
}
