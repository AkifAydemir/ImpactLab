namespace ImpactLab.Core.IO.Migrations;

public static class MigrationCoverageValidator
{
    public static IReadOnlyList<string> Validate(
        MigrationMatrix m,
        string type,
        int oldest,
        int current
    )
    {
        var issues = new List<string>();
        for (var v = oldest; v < current; v++)
            if (!m.Covers(type, v, v + 1))
                issues.Add($"Missing migration {type} {v}->{v + 1}");
        return issues;
    }
}
