using ImpactLab.Core.Continuum;

namespace ImpactLab.Core.Thermal.Coupled;

public interface IMonolithicThermoMechanicalAssembler
{
    MonolithicAssemblyResult Assemble(
        TetrahedralMesh mesh,
        CoupledState state,
        CoupledState previous,
        double dt,
        CancellationToken ct = default
    );
    void Commit();
    void Revert();
}
