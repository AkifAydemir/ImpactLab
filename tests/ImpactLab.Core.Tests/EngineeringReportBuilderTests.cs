using ImpactLab.Core.Analysis;
using ImpactLab.Core.Reporting;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class EngineeringReportBuilderTests
{
    [Fact]
    public void BuildsRunSummarySection()
    {
        var r = new RunResultSummary(1, 0, 0, .01, 0, 0, 0, 0, []);
        var d = EngineeringReportBuilder.Build(
            "S",
            "B",
            new BackendResultSummary("B", "Lattice", r, null)
        );
        Assert.NotEmpty(d.Sections);
    }
}
