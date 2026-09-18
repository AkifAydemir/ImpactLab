using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record ElementTrialState(
    StressTensor6 Stress,
    StrainTensor6 Strain,
    PlasticState Plastic,
    double PlasticDissipationJ,
    double TemperatureKelvin
);
