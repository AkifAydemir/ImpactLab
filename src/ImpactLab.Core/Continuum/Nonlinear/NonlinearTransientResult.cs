namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearTransientResult(
    TetrahedralMesh Mesh,
    IReadOnlyList<NonlinearTransientFrame> Frames,
    IReadOnlyList<NonlinearStepMetrics> Steps
);
