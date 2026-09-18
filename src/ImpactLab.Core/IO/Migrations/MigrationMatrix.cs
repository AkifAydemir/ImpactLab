namespace ImpactLab.Core.IO.Migrations;

public sealed class MigrationMatrix
{
    private readonly List<MigrationMatrixEntry> _e = [];

    public void Add(MigrationMatrixEntry e) => _e.Add(e);

    public IReadOnlyList<MigrationMatrixEntry> Entries => _e;

    public bool Covers(string type, int from, int to) =>
        _e.Any(x => x.DocumentType == type && x.FromVersion == from && x.ToVersion == to);
}
