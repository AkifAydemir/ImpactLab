using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Dynamics;

public static class ContinuumDampingMatrixFactory
{
    public static SparseCsrMatrix Rayleigh(
        SparseCsrMatrix m,
        SparseCsrMatrix k,
        ContinuumDampingSettings s
    ) => SparseMatrixAlgebra.Combine((m, s.MassCoefficient), (k, s.StiffnessCoefficient));
}
