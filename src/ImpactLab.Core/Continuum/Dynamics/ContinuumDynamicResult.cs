namespace ImpactLab.Core.Continuum.Dynamics;

public sealed record ContinuumDynamicResult(
    TetrahedralMesh Mesh,
    IReadOnlyList<ContinuumDynamicFrame> Frames,
    IReadOnlyList<DynamicStepDiagnostics> Diagnostics,
    TimeSpan ComputeTime
);
