namespace ImpactLab.Core.Finalization;

public sealed record ReleaseGateResult
{
    public ReleaseGateResult(
        ReleaseGateKind gate,
        ReleaseGateStatus status,
        string summary,
        IReadOnlyList<string> findings
    )
    {
        Gate = gate;
        Status = status;
        Summary = summary;
        Findings = findings;
    }

    public ReleaseGateResult(
        ReleaseGateKind gate,
        bool passed,
        string summary,
        IReadOnlyList<string> findings
    )
        : this(
            gate,
            passed ? ReleaseGateStatus.Passed : ReleaseGateStatus.Failed,
            summary,
            findings
        ) { }

    public ReleaseGateKind Gate { get; }
    public ReleaseGateStatus Status { get; }
    public string Summary { get; }
    public IReadOnlyList<string> Findings { get; }
    public bool Passed => Status == ReleaseGateStatus.Passed;
    public bool Blocking => !Passed;

    public void Deconstruct(
        out ReleaseGateKind gate,
        out bool passed,
        out string summary,
        out IReadOnlyList<string> findings
    )
    {
        gate = Gate;
        passed = Passed;
        summary = Summary;
        findings = Findings;
    }
}
