namespace ImpactLab.Core.Verification;

public enum RunValidityStatus
{
    Passed,
    Warning,
    Failed,
}

public sealed record RunValidityCheck(
    string Id,
    string Name,
    RunValidityStatus Status,
    double? Observed,
    double? Limit,
    string Unit,
    string Detail
);

public sealed record RunValidityAssessment(IReadOnlyList<RunValidityCheck> Checks)
{
    public bool HasFailure => Checks.Any(x => x.Status == RunValidityStatus.Failed);
    public int PassedCount => Checks.Count(x => x.Status == RunValidityStatus.Passed);
    public int WarningCount => Checks.Count(x => x.Status == RunValidityStatus.Warning);
    public int FailedCount => Checks.Count(x => x.Status == RunValidityStatus.Failed);
}
