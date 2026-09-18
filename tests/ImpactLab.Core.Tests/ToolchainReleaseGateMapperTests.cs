using ImpactLab.Core.Finalization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ToolchainReleaseGateMapperTests
{
    [Fact]
    public void MissingSdkBlocksBuildAndTestsWithoutCallingEitherAFailure()
    {
        var evidence = Evidence(
            "environment-blocked",
            false,
            new ToolchainVerificationStage(
                "sdk",
                "environment-blocked",
                127,
                ".NET SDK executable was not found on PATH."
            )
        );

        var gates = ToolchainReleaseGateMapper.Map(evidence);

        Assert.Equal(
            ReleaseGateStatus.EnvironmentBlocked,
            gates.Single(x => x.Gate == ReleaseGateKind.Build).Status
        );
        Assert.Equal(
            ReleaseGateStatus.EnvironmentBlocked,
            gates.Single(x => x.Gate == ReleaseGateKind.Tests).Status
        );
    }

    [Fact]
    public void BrokenSdkQueryIsEnvironmentBlockedRatherThanBuildFailure()
    {
        var evidence = Evidence(
            "environment-blocked",
            false,
            new ToolchainVerificationStage("sdk", "environment-blocked", 139, "SDK query failed.")
        );

        var gates = ToolchainReleaseGateMapper.Map(evidence);

        Assert.All(gates, gate => Assert.Equal(ReleaseGateStatus.EnvironmentBlocked, gate.Status));
    }

    [Fact]
    public void RestoreFailureFailsBuildGateButLeavesTestsNotRun()
    {
        var evidence = Evidence(
            "failed",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("restore", "failed", 1, "Restore failed.")
        );

        var gates = ToolchainReleaseGateMapper.Map(evidence);

        Assert.Equal(
            ReleaseGateStatus.Failed,
            gates.Single(x => x.Gate == ReleaseGateKind.Build).Status
        );
        Assert.Equal(
            ReleaseGateStatus.NotRun,
            gates.Single(x => x.Gate == ReleaseGateKind.Tests).Status
        );
    }

    [Fact]
    public void PassingBuildAndFailingTestsRemainDistinct()
    {
        var evidence = Evidence(
            "failed",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("restore", "passed", 0, "Restore passed."),
            new("build", "passed", 0, "Build passed."),
            new("tests", "failed", 1, "Tests failed.")
        );

        var gates = ToolchainReleaseGateMapper.Map(evidence);

        Assert.Equal(
            ReleaseGateStatus.Passed,
            gates.Single(x => x.Gate == ReleaseGateKind.Build).Status
        );
        Assert.Equal(
            ReleaseGateStatus.Failed,
            gates.Single(x => x.Gate == ReleaseGateKind.Tests).Status
        );
    }

    [Fact]
    public void EvidenceStoreLoadsCanonicalHarnessJson()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(
                path,
                "{\"schemaVersion\":1,\"generatedUtc\":\"2026-09-01T08:32:11Z\",\"overallStatus\":\"environment-blocked\",\"dotnet\":{\"available\":false,\"version\":null},\"stages\":[{\"name\":\"sdk\",\"status\":\"environment-blocked\",\"exitCode\":127,\"summary\":\"SDK unavailable.\"}],\"error\":null}"
            );

            var evidence = ToolchainVerificationEvidenceStore.Load(path);

            Assert.Equal("environment-blocked", evidence.OverallStatus);
            Assert.False(evidence.Dotnet.Available);
            Assert.Single(evidence.Stages);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void EvidenceValidationRejectsDuplicateStages()
    {
        var evidence = Evidence(
            "failed",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("restore", "failed", 1, "Restore failed."),
            new("restore", "failed", 1, "Duplicate restore.")
        );

        Assert.Throws<InvalidDataException>(() =>
            ToolchainVerificationEvidenceStore.Validate(evidence)
        );
    }

    [Fact]
    public void EvidenceValidationRejectsContradictoryOverallPass()
    {
        var evidence = Evidence(
            "passed",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("restore", "passed", 0, "Restore passed."),
            new("build", "passed", 0, "Build passed.")
        );

        Assert.Throws<InvalidDataException>(() =>
            ToolchainVerificationEvidenceStore.Validate(evidence)
        );
    }

    [Fact]
    public void EvidenceValidationAcceptsCanonicalEnvironmentBlock()
    {
        var evidence = Evidence(
            "environment-blocked",
            false,
            new ToolchainVerificationStage(
                "sdk",
                "environment-blocked",
                127,
                ".NET SDK executable was not found on PATH."
            )
        );

        ToolchainVerificationEvidenceStore.Validate(evidence);
    }

    [Fact]
    public void EvidenceValidationAcceptsSignedNativeFailureExitCodes()
    {
        var evidence = Evidence(
            "environment-blocked",
            false,
            new ToolchainVerificationStage(
                "sdk",
                "environment-blocked",
                unchecked((int)0xC0000005),
                "SDK process crashed."
            )
        );

        ToolchainVerificationEvidenceStore.Validate(evidence);
    }

    [Fact]
    public void EvidenceValidationRejectsUnknownOrOutOfOrderStages()
    {
        var unknown = Evidence(
            "not-run",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("compile", "passed", 0, "Unknown stage.")
        );
        var outOfOrder = Evidence(
            "not-run",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("build", "passed", 0, "Build appeared before restore.")
        );

        Assert.Throws<InvalidDataException>(() =>
            ToolchainVerificationEvidenceStore.Validate(unknown)
        );
        Assert.Throws<InvalidDataException>(() =>
            ToolchainVerificationEvidenceStore.Validate(outOfOrder)
        );
    }

    [Fact]
    public void EvidenceValidationRejectsStagesAfterTerminalFailure()
    {
        var evidence = Evidence(
            "failed",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("restore", "failed", 1, "Restore failed."),
            new("build", "failed", 1, "Build must not run.")
        );

        Assert.Throws<InvalidDataException>(() =>
            ToolchainVerificationEvidenceStore.Validate(evidence)
        );
    }

    [Fact]
    public void EvidenceValidationAcceptsCompletePassingPipeline()
    {
        var evidence = Evidence(
            "passed",
            true,
            new("sdk", "passed", 0, "SDK available."),
            new("restore", "passed", 0, "Restore passed."),
            new("build", "passed", 0, "Build passed."),
            new("tests", "passed", 0, "Tests passed.")
        );

        ToolchainVerificationEvidenceStore.Validate(evidence);
    }

    [Fact]
    public void EvidenceLoadRejectsNullStageElement()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(
                path,
                "{\"schemaVersion\":1,\"generatedUtc\":\"2026-09-06T15:00:00Z\",\"overallStatus\":\"environment-blocked\",\"dotnet\":{\"available\":false,\"version\":null},\"stages\":[null],\"error\":null}"
            );

            Assert.Throws<InvalidDataException>(() =>
                ToolchainVerificationEvidenceStore.Load(path)
            );
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static ToolchainVerificationEvidence Evidence(
        string overallStatus,
        bool dotnetAvailable,
        params ToolchainVerificationStage[] stages
    ) =>
        new(
            1,
            DateTimeOffset.UtcNow,
            overallStatus,
            new(dotnetAvailable, dotnetAvailable ? "8.0.100" : null),
            stages,
            null
        );
}
