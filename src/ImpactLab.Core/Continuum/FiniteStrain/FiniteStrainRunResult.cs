namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainRunResult(
    TetrahedralMesh Mesh,
    IReadOnlyList<FiniteStrainRunFrame> Frames,
    TimeSpan ComputeTime,
    IReadOnlyList<string> Warnings
);
