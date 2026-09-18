namespace ImpactLab.Core.Verification;

public sealed record VerificationSignalResult(
    string ReferenceId,
    VerificationMetricSet Metrics,
    bool Passed
);

public sealed record VerificationResult(
    string CaseId,
    string CaseName,
    IReadOnlyList<VerificationSignalResult> Signals,
    string? Error = null
)
{
    public bool Passed => Error is null && Signals.All(x => x.Passed);
}
