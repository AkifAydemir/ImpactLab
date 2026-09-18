namespace ImpactLab.Core.PostProcessing;

public sealed record SectionFrame(
    CutPlaneDefinition Plane,
    double TimeSeconds,
    IReadOnlyList<SectionNodeSample> Nodes
);
