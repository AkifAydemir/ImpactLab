using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Results;

public sealed record ContinuumResult(
    TetrahedralMesh Mesh,
    IReadOnlyList<ContinuumFrame> Frames,
    IReadOnlyList<LinearSolveReport> LinearSolves,
    TimeSpan ComputeTime
);
