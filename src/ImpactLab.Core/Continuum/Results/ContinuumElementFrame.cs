using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Continuum.Results;

public sealed record ContinuumElementFrame(
    StressTensor6[] Stress,
    StrainTensor6[] Strain,
    double[] VonMisesPa
);
