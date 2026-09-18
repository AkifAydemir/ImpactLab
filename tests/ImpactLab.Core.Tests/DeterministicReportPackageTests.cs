using System.Globalization;
using ImpactLab.Core.Reporting;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class DeterministicReportPackageTests
{
    [Fact]
    public void PackageManifestVerifies()
    {
        var doc = new EngineeringReportDocument(
            "Report",
            "Fixture",
            "backend",
            "2026-08-31T00:00:00.0000000+00:00",
            [new("Summary", "Stable narrative.", [new("Value", "42", "kN")], [])]
        );
        var ctx = new ReportGenerationContext(
            "Fixture",
            "fixture-sha",
            "backend",
            "v15",
            DateTimeOffset.Parse("2026-08-31T00:00:00+00:00", CultureInfo.InvariantCulture),
            new Dictionary<string, string>()
        );
        var dir = Path.Combine(
            Path.GetTempPath(),
            "impactlab-report-" + Guid.NewGuid().ToString("N")
        );
        try
        {
            new DeterministicReportPackageBuilder().Build(dir, doc, ctx, "engineering-standard");
            Assert.True(ReportGoldenPackageVerifier.Verify(dir).Passed);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }
}
