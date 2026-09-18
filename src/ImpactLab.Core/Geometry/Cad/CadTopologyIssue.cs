namespace ImpactLab.Core.Geometry.Cad;

public sealed record CadTopologyIssue(
    CadTopologyIssueKind Kind,
    string Message,
    IReadOnlyList<CadEntityId> Entities,
    double Severity
);
