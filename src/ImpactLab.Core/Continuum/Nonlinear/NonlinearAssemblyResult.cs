using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearAssemblyResult(
    SparseCsrMatrix Tangent,
    double[] InternalForce,
    double InternalEnergy,
    double PlasticDissipation,
    int PlasticElements
);
