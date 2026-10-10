namespace ImpactLab.Core.Mobility;

public sealed class TerrainGridMap
{
    private readonly TerrainCellObservation[,] _cells;

    public TerrainGridMap(int rows, int columns, IEnumerable<TerrainCellObservation> cells)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(rows));
        Rows = rows;
        Columns = columns;
        _cells = new TerrainCellObservation[rows, columns];
        var seen = new bool[rows, columns];
        foreach (var cell in cells)
        {
            cell.Validate();
            if (cell.Row >= rows || cell.Column >= columns)
                throw new ArgumentOutOfRangeException(
                    nameof(cells),
                    "Terrain cell is outside the grid."
                );
            if (seen[cell.Row, cell.Column])
                throw new InvalidOperationException(
                    $"Duplicate terrain cell {cell.Row},{cell.Column}."
                );
            _cells[cell.Row, cell.Column] = cell;
            seen[cell.Row, cell.Column] = true;
        }
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
            if (!seen[row, column])
                throw new InvalidOperationException($"Missing terrain cell {row},{column}.");
    }

    public int Rows { get; }
    public int Columns { get; }
    public TerrainCellObservation this[int row, int column] => _cells[row, column];
}
