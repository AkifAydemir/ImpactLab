using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Thermal.Coupled;

public sealed record CoupledBlockSystem(
    SparseCsrMatrix Kuu,
    SparseCsrMatrix KuT,
    SparseCsrMatrix KTu,
    SparseCsrMatrix KTT,
    double[] MechanicalResidual,
    double[] ThermalResidual
);
