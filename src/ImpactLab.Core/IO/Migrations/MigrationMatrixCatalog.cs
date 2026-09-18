namespace ImpactLab.Core.IO.Migrations;

public static class MigrationMatrixCatalog
{
    public static MigrationMatrix CreateV15()
    {
        var m = new MigrationMatrix();
        foreach (var type in new[] { "scenario", "experiment", "workspace" })
        {
            m.Add(new(type, 3, 4, $"{type}-v3-v4", true, true));
            m.Add(new(type, 4, 5, $"{type}-v4-v5", true, true));
        }
        return m;
    }
}
