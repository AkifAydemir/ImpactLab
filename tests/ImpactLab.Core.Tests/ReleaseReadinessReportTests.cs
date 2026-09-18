using ImpactLab.Core.Finalization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ReleaseReadinessReportTests
{
    [Fact]
    public void EmptyGateSetIsNeverReleaseReady()
    {
        var report = new ReleaseReadinessReport(DateTimeOffset.UtcNow, []);

        Assert.False(report.HasCompleteGateSet);
        Assert.False(report.Ready);
        Assert.Equal(Enum.GetValues<ReleaseGateKind>().Length, report.MissingGates.Count);
        Assert.Empty(report.DuplicateGates);
    }

    [Fact]
    public void PartialOrDuplicatedGateSetsAreNeverReleaseReady()
    {
        var required = Enum.GetValues<ReleaseGateKind>();
        var partial = required
            .Skip(1)
            .Select(kind => new ReleaseGateResult(
                kind,
                ReleaseGateStatus.Passed,
                kind.ToString(),
                []
            ))
            .ToArray();
        var duplicate = required
            .Select(kind => new ReleaseGateResult(
                kind,
                ReleaseGateStatus.Passed,
                kind.ToString(),
                []
            ))
            .Append(new ReleaseGateResult(required[0], ReleaseGateStatus.Passed, "duplicate", []))
            .ToArray();

        var partialReport = new ReleaseReadinessReport(DateTimeOffset.UtcNow, partial);
        var duplicateReport = new ReleaseReadinessReport(DateTimeOffset.UtcNow, duplicate);

        Assert.False(partialReport.HasCompleteGateSet);
        Assert.Single(partialReport.MissingGates);
        Assert.Equal(required[0], partialReport.MissingGates[0]);
        Assert.Empty(partialReport.DuplicateGates);

        Assert.False(duplicateReport.HasCompleteGateSet);
        Assert.Empty(duplicateReport.MissingGates);
        Assert.Single(duplicateReport.DuplicateGates);
        Assert.Equal(required[0], duplicateReport.DuplicateGates[0]);
    }

    [Fact]
    public void ReleaseGateResultPreservesLegacyBoolDeconstruction()
    {
        var result = new ReleaseGateResult(
            ReleaseGateKind.Build,
            ReleaseGateStatus.EnvironmentBlocked,
            "blocked",
            []
        );

        var (gate, passed, summary, findings) = result;

        Assert.Equal(ReleaseGateKind.Build, gate);
        Assert.False(passed);
        Assert.Equal("blocked", summary);
        Assert.Empty(findings);
    }

    [Fact]
    public void ExistingReleaseGateNumericValuesRemainCompatible()
    {
        Assert.Equal(0, (int)ReleaseGateKind.FeatureCompleteness);
        Assert.Equal(4, (int)ReleaseGateKind.GoldenFixtures);
        Assert.Equal(9, (int)ReleaseGateKind.AccessibilityReview);
        Assert.Equal(10, (int)ReleaseGateKind.NumericalVerification);
        Assert.Equal(12, (int)ReleaseGateKind.RuntimeIntegration);
    }

    [Fact]
    public void EnvironmentBlockedIsDistinctFromFailureAndBlocksRelease()
    {
        var gates = Enum.GetValues<ReleaseGateKind>()
            .Select(kind => new ReleaseGateResult(
                kind,
                kind == ReleaseGateKind.Build
                    ? ReleaseGateStatus.EnvironmentBlocked
                    : ReleaseGateStatus.Passed,
                kind.ToString(),
                []
            ))
            .ToArray();
        var report = new ReleaseReadinessReport(DateTimeOffset.UtcNow, gates);

        Assert.True(report.HasCompleteGateSet);
        Assert.False(report.Ready);
        Assert.Single(report.EnvironmentBlocked);
        Assert.Empty(report.Failed);
        Assert.Equal(ReleaseGateKind.Build, report.EnvironmentBlocked[0].Gate);
    }

    [Fact]
    public void EmptyOrPartialFeatureMatrixIsNeverComplete()
    {
        var empty = new FeatureCompletenessMatrix();
        var partial = new FeatureCompletenessMatrix();
        partial.Set(
            new FeatureReadinessItem(
                FeatureArea.ScenarioAuthoring,
                FeatureAreaStatus.Ready,
                "partial",
                null
            )
        );

        Assert.False(empty.HasCompleteAreaSet);
        Assert.Equal(Enum.GetValues<FeatureArea>().Length, empty.MissingAreas.Count);
        Assert.False(empty.IsSourceFeatureComplete);
        Assert.False(empty.IsFeatureComplete);
        Assert.False(partial.HasCompleteAreaSet);
        Assert.Equal(Enum.GetValues<FeatureArea>().Length - 1, partial.MissingAreas.Count);
        Assert.DoesNotContain(FeatureArea.ScenarioAuthoring, partial.MissingAreas);
        Assert.False(partial.IsSourceFeatureComplete);
        Assert.False(partial.IsFeatureComplete);
    }

    [Fact]
    public void CanonicalSourceBaselineIsCompleteAsSourceButNotReleaseFeatureComplete()
    {
        var matrix = FeatureCompletenessReview.CreateV16SourceBaseline();

        Assert.True(matrix.HasCompleteAreaSet);
        Assert.True(matrix.IsSourceFeatureComplete);
        Assert.False(matrix.IsFeatureComplete);
    }

    [Fact]
    public void CompletePassedGateSetIsReleaseReady()
    {
        var gates = Enum.GetValues<ReleaseGateKind>()
            .Select(kind => new ReleaseGateResult(
                kind,
                ReleaseGateStatus.Passed,
                kind.ToString(),
                []
            ))
            .ToArray();
        var report = new ReleaseReadinessReport(DateTimeOffset.UtcNow, gates);

        Assert.True(report.HasCompleteGateSet);
        Assert.True(report.Ready);
        Assert.Empty(report.Blocking);
    }
}
