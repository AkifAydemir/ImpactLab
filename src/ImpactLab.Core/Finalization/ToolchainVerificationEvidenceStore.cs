using System.Text.Json;

namespace ImpactLab.Core.Finalization;

public static class ToolchainVerificationEvidenceStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly string[] StageOrder = ["sdk", "restore", "build", "tests"];

    private static readonly HashSet<string> AllowedOverallStatuses = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "not-run",
        "passed",
        "failed",
        "environment-blocked",
    };

    private static readonly HashSet<string> AllowedStageStatuses = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "passed",
        "failed",
        "environment-blocked",
    };

    public static ToolchainVerificationEvidence Load(string path)
    {
        var evidence = JsonSerializer.Deserialize<ToolchainVerificationEvidence>(
            File.ReadAllText(path),
            Options
        );

        if (evidence is null)
            throw new InvalidDataException("Toolchain verification evidence is empty.");

        Validate(evidence);
        return evidence;
    }

    public static void Validate(ToolchainVerificationEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        if (evidence.SchemaVersion != 1)
            throw new InvalidDataException(
                $"Unsupported toolchain evidence schema {evidence.SchemaVersion}."
            );
        if (evidence.Dotnet is null)
            throw new InvalidDataException(
                "Toolchain verification evidence has no dotnet descriptor."
            );
        if (evidence.Stages is null)
            throw new InvalidDataException("Toolchain verification evidence has no stage list.");
        if (!AllowedOverallStatuses.Contains(evidence.OverallStatus))
            throw new InvalidDataException(
                $"Unknown overall toolchain status '{evidence.OverallStatus}'."
            );
        if (evidence.Stages.Count == 0)
            throw new InvalidDataException("Toolchain verification evidence has no sdk stage.");
        if (evidence.Stages.Count > StageOrder.Length)
            throw new InvalidDataException(
                "Toolchain verification evidence contains more stages than the canonical pipeline."
            );

        for (var i = 0; i < evidence.Stages.Count; i++)
        {
            var stage = evidence.Stages[i];
            var expectedName = StageOrder[i];

            if (stage is null)
                throw new InvalidDataException($"Toolchain stage {i + 1} is null.");
            if (!string.Equals(stage.Name, expectedName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Toolchain stage {i + 1} must be '{expectedName}', not '{stage.Name}'."
                );
            if (!AllowedStageStatuses.Contains(stage.Status))
                throw new InvalidDataException(
                    $"Unknown status '{stage.Status}' for stage '{stage.Name}'."
                );
            if (string.IsNullOrWhiteSpace(stage.Summary))
                throw new InvalidDataException($"Stage '{stage.Name}' has no summary.");
            if (
                stage.Status.Equals("passed", StringComparison.OrdinalIgnoreCase)
                && stage.ExitCode != 0
            )
                throw new InvalidDataException(
                    $"Passed stage '{stage.Name}' has non-zero exit code {stage.ExitCode}."
                );
            if (
                !stage.Status.Equals("passed", StringComparison.OrdinalIgnoreCase)
                && stage.ExitCode == 0
            )
                throw new InvalidDataException(
                    $"Non-passed stage '{stage.Name}' has zero exit code."
                );

            // The canonical verify pipeline is sequential. A failed or blocked stage terminates it,
            // so every stage before the final recorded stage must have passed.
            if (
                i < evidence.Stages.Count - 1
                && !stage.Status.Equals("passed", StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new InvalidDataException(
                    $"Terminal stage '{stage.Name}' cannot be followed by later toolchain stages."
                );
            }
        }

        var sdk = evidence.Stages[0];
        if (sdk.Status.Equals("passed", StringComparison.OrdinalIgnoreCase))
        {
            if (!evidence.Dotnet.Available)
                throw new InvalidDataException(
                    "A passed sdk stage requires dotnet.available=true."
                );
            if (string.IsNullOrWhiteSpace(evidence.Dotnet.Version))
                throw new InvalidDataException("Available .NET SDK evidence has no version.");
        }
        else if (sdk.Status.Equals("environment-blocked", StringComparison.OrdinalIgnoreCase))
        {
            if (evidence.Dotnet.Available)
                throw new InvalidDataException(
                    "An environment-blocked sdk stage requires dotnet.available=false."
                );
            if (!string.IsNullOrWhiteSpace(evidence.Dotnet.Version))
                throw new InvalidDataException("Unavailable .NET SDK must not report a version.");
        }
        else
        {
            throw new InvalidDataException(
                "The sdk stage must either pass or be environment-blocked."
            );
        }

        var finalStage = evidence.Stages[^1];
        var pipelinePassed =
            evidence.Stages.Count == StageOrder.Length && evidence.Stages.All(IsPassed);
        var expectedOverall =
            pipelinePassed ? "passed"
            : finalStage.Status.Equals("failed", StringComparison.OrdinalIgnoreCase) ? "failed"
            : finalStage.Status.Equals("environment-blocked", StringComparison.OrdinalIgnoreCase)
                ? "environment-blocked"
            : "not-run";

        if (!evidence.OverallStatus.Equals(expectedOverall, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Overall status '{evidence.OverallStatus}' contradicts the canonical pipeline state '{expectedOverall}'."
            );
        }
    }

    private static bool IsPassed(ToolchainVerificationStage stage) =>
        stage.ExitCode == 0 && stage.Status.Equals("passed", StringComparison.OrdinalIgnoreCase);
}
