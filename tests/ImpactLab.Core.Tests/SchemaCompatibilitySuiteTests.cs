using ImpactLab.Core.Persistence;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SchemaCompatibilitySuiteTests
{
    [Fact]
    public void ReportsMigrationOutput()
    {
        var p = Path.GetTempFileName();
        File.WriteAllText(p, "{}");
        var r = new SchemaCompatibilitySuite().Run(
            [new("x", "scenario", 1, 2, p)],
            (s, a, b) => "{\"ok\":true}"
        );
        Assert.True(r.Single().Passed);
    }
}
