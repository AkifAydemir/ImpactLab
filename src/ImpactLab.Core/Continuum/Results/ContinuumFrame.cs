using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Results;

public sealed record ContinuumFrame(
    double TimeSeconds,
    Vec3[] Position,
    Vec3[] Displacement,
    double[] TemperatureKelvin,
    ContinuumElementFrame Elements
);
