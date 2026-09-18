namespace ImpactLab.Core.Finalization;

public static class ToolchainReleaseGateMapper
{
    public static IReadOnlyList<ReleaseGateResult> Map(ToolchainVerificationEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ToolchainVerificationEvidenceStore.Validate(evidence);

        var sdk = evidence.FindStage("sdk");
        var restore = evidence.FindStage("restore");
        var build = evidence.FindStage("build");
        var tests = evidence.FindStage("tests");

        if (
            IsEnvironmentBlocked(sdk)
            || (
                !evidence.Dotnet.Available
                && evidence.OverallStatus.Equals(
                    "environment-blocked",
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            return
            [
                Result(
                    ReleaseGateKind.Build,
                    ReleaseGateStatus.EnvironmentBlocked,
                    sdk?.Summary ?? ".NET SDK is unavailable; build could not execute."
                ),
                Result(
                    ReleaseGateKind.Tests,
                    ReleaseGateStatus.EnvironmentBlocked,
                    ".NET SDK is unavailable; tests could not execute."
                ),
            ];
        }

        var buildResult = MapBuild(restore, build);
        var testResult = MapTests(buildResult, tests);
        return [buildResult, testResult];
    }

    private static ReleaseGateResult MapBuild(
        ToolchainVerificationStage? restore,
        ToolchainVerificationStage? build
    )
    {
        if (IsEnvironmentBlocked(restore))
            return Result(
                ReleaseGateKind.Build,
                ReleaseGateStatus.EnvironmentBlocked,
                restore!.Summary
            );
        if (IsFailed(restore))
            return Result(
                ReleaseGateKind.Build,
                ReleaseGateStatus.Failed,
                $"Restore failed before compilation: {restore!.Summary}"
            );

        if (build is null)
            return Result(
                ReleaseGateKind.Build,
                ReleaseGateStatus.NotRun,
                "Build stage has no executed evidence."
            );
        if (IsEnvironmentBlocked(build))
            return Result(
                ReleaseGateKind.Build,
                ReleaseGateStatus.EnvironmentBlocked,
                build.Summary
            );
        if (IsPassed(build))
            return Result(ReleaseGateKind.Build, ReleaseGateStatus.Passed, build.Summary);
        if (IsFailed(build))
            return Result(ReleaseGateKind.Build, ReleaseGateStatus.Failed, build.Summary);

        return Result(
            ReleaseGateKind.Build,
            ReleaseGateStatus.NotRun,
            $"Build stage status '{build.Status}' is not executed pass/fail evidence."
        );
    }

    private static ReleaseGateResult MapTests(
        ReleaseGateResult build,
        ToolchainVerificationStage? tests
    )
    {
        if (build.Status == ReleaseGateStatus.EnvironmentBlocked)
            return Result(
                ReleaseGateKind.Tests,
                ReleaseGateStatus.EnvironmentBlocked,
                "Tests could not execute because the required build environment was unavailable."
            );
        if (!build.Passed)
            return Result(
                ReleaseGateKind.Tests,
                ReleaseGateStatus.NotRun,
                "Tests did not execute after the build gate failed or remained unexecuted."
            );

        if (tests is null)
            return Result(
                ReleaseGateKind.Tests,
                ReleaseGateStatus.NotRun,
                "Test stage has no executed evidence."
            );
        if (IsEnvironmentBlocked(tests))
            return Result(
                ReleaseGateKind.Tests,
                ReleaseGateStatus.EnvironmentBlocked,
                tests.Summary
            );
        if (IsPassed(tests))
            return Result(ReleaseGateKind.Tests, ReleaseGateStatus.Passed, tests.Summary);
        if (IsFailed(tests))
            return Result(ReleaseGateKind.Tests, ReleaseGateStatus.Failed, tests.Summary);

        return Result(
            ReleaseGateKind.Tests,
            ReleaseGateStatus.NotRun,
            $"Test stage status '{tests.Status}' is not executed pass/fail evidence."
        );
    }

    private static bool IsPassed(ToolchainVerificationStage? stage) =>
        stage is not null
        && stage.ExitCode == 0
        && stage.Status.Equals("passed", StringComparison.OrdinalIgnoreCase);

    private static bool IsFailed(ToolchainVerificationStage? stage) =>
        stage is not null
        && (
            stage.ExitCode != 0 || stage.Status.Equals("failed", StringComparison.OrdinalIgnoreCase)
        )
        && !IsEnvironmentBlocked(stage);

    private static bool IsEnvironmentBlocked(ToolchainVerificationStage? stage) =>
        stage is not null
        && stage.Status.Equals("environment-blocked", StringComparison.OrdinalIgnoreCase);

    private static ReleaseGateResult Result(
        ReleaseGateKind gate,
        ReleaseGateStatus status,
        string summary
    ) => new(gate, status, summary, []);
}
