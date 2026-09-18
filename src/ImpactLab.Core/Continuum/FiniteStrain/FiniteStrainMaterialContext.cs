using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainMaterialContext(
    MaterialDefinition Material,
    FiniteStrainElementState Previous,
    FiniteStrainKinematics Kinematics,
    double TimeStepSeconds,
    double TemperatureKelvin
);
