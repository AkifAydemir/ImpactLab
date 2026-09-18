namespace ImpactLab.Core.Finalization;

public sealed record ToolchainVerificationStage(
    string Name,
    string Status,
    int ExitCode,
    string Summary
);
