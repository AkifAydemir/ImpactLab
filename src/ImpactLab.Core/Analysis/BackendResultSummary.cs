namespace ImpactLab.Core.Analysis;

public sealed record BackendResultSummary(
    string BackendId,
    string DomainKind,
    RunResultSummary Compatibility,
    ContinuumResultSummary? Continuum
);
