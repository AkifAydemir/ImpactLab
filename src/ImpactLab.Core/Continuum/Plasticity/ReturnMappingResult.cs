using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Continuum.Plasticity;

public readonly record struct ReturnMappingResult(
    StressTensor6 Stress,
    PlasticState State,
    bool Yielded,
    double PlasticMultiplier,
    double ConsistentTangentScale,
    double DissipationDensityJPerM3
);
