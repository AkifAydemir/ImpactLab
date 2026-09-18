using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Contact;

public sealed record MortarContactContribution(
    double[] Residual,
    SparseCsrMatrix Tangent,
    double ContactEnergy,
    int ActiveConstraints
);
