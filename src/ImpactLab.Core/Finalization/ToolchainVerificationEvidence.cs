namespace ImpactLab.Core.Finalization;

public sealed record ToolchainVerificationEvidence(
    int SchemaVersion,
    DateTimeOffset GeneratedUtc,
    string OverallStatus,
    ToolchainDescriptor Dotnet,
    IReadOnlyList<ToolchainVerificationStage> Stages,
    string? Error
)
{
    public ToolchainVerificationStage? FindStage(string name) =>
        Stages.LastOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
}
