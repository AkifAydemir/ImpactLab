namespace ImpactLab.Core.Continuum.Static;

public sealed record ContinuumStaticSettings(
    string SparseSolverId = "managed-cg",
    double LinearTolerance = 1e-9,
    int MaxIterations = 20000,
    bool PreferNative = false
);
