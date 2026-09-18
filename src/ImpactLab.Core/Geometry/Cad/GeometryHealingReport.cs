namespace ImpactLab.Core.Geometry.Cad;

public sealed record GeometryHealingReport(
    CadModel Model,
    int StitchedEdges,
    int RemovedEdges,
    int RemovedFaces,
    int ReorientedFaces,
    IReadOnlyList<CadTopologyIssue> RemainingIssues
);
