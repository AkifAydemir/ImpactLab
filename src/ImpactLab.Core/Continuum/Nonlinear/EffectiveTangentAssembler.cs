using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Nonlinear;

public static class EffectiveTangentAssembler
{
    public static EffectiveDynamicTangent Assemble(
        SparseCsrMatrix mass,
        SparseCsrMatrix damping,
        SparseCsrMatrix tangent,
        double dt,
        double beta,
        double gamma
    )
    {
        var a0 = 1.0 / (beta * dt * dt);
        var a1 = gamma / (beta * dt);
        return new(
            SparseMatrixAlgebra.Combine((mass, a0), (damping, a1), (tangent, 1.0)),
            a0,
            a1,
            1.0
        );
    }
}
