using ImpactLab.Core.Continuum.Plasticity;

namespace ImpactLab.Core.Continuum.Nonlinear;

public interface INonlinearContinuumAssembler
{
    NonlinearAssemblyResult Assemble(
        TetrahedralMesh mesh,
        ReadOnlySpan<double> displacement,
        IReadOnlyList<PlasticState> committed,
        double dt,
        double timeSeconds,
        CancellationToken ct = default
    );
    IReadOnlyList<PlasticState> Commit();
    void Revert();
}
