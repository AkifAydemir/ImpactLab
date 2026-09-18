using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainRunFrame(
    double TimeSeconds,
    IReadOnlyList<Vec3> Position,
    IReadOnlyList<double> TemperatureKelvin,
    IReadOnlyList<FiniteStrainElementState> ElementState,
    double InternalEnergyJ,
    double PlasticDissipationJ
);
