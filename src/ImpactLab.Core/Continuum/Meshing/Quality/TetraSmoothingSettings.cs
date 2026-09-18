namespace ImpactLab.Core.Continuum.Meshing.Quality;

public sealed record TetraSmoothingSettings(
    int Iterations = 10,
    double Relaxation = 0.4,
    bool PreserveBoundary = true,
    double MinimumMeanRatio = 0.05
);
