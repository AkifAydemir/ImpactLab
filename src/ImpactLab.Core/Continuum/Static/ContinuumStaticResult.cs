using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Static;

public sealed record ContinuumStaticResult(
    Vec3[] Displacement,
    StressTensor6[] ElementStress,
    StrainTensor6[] ElementStrain,
    LinearSolveReport LinearSolve,
    double LoadFactor
);
