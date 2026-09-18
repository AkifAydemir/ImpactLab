using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record EffectiveDynamicTangent(
    SparseCsrMatrix Matrix,
    double MassScale,
    double DampingScale,
    double StiffnessScale
);
