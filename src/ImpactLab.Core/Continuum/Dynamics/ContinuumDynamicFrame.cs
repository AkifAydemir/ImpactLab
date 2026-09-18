using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Dynamics;

public sealed record ContinuumDynamicFrame(
    double TimeSeconds,
    Vec3[] Position,
    Vec3[] Displacement,
    Vec3[] Velocity,
    Vec3[] Acceleration,
    double[] TemperatureKelvin,
    StressTensor6[] ElementStress,
    StrainTensor6[] ElementStrain,
    double[] EquivalentPlasticStrain,
    DynamicStepDiagnostics Diagnostics
);
