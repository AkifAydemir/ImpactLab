namespace ImpactLab.Core.Geometry.Repair;

public sealed record TopologyHealingReport(
    int InputVertices,
    int InputTriangles,
    int OutputVertices,
    int OutputTriangles,
    int WeldedVertices,
    int RemovedDegenerates,
    int PatchedHoles,
    int FlippedFaces,
    int NonManifoldEdgesRemaining,
    IReadOnlyList<string> Warnings
);
