namespace ImpactLab.Core.Continuum.Meshing;

public sealed record MeshQualitySummary(
    int Elements,
    int Inverted,
    double MinMeanRatio,
    double MeanMeanRatio,
    double MinDihedral,
    double MaxDihedral
);
