namespace ImpactLab.Core.Geometry.Cad;

public sealed record CadImportReport(
    bool Success,
    CadModel? Model,
    IReadOnlyList<CadTopologyIssue> Issues,
    string? Error
);
