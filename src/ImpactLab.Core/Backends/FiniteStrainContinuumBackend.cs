using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.FiniteStrain;

namespace ImpactLab.Core.Backends;

public sealed class FiniteStrainContinuumBackend : ISimulationBackend
{
    public const string BackendId = "finite-strain-continuum-v1";
    public SimulationBackendDescriptor Descriptor { get; } =
        new(
            BackendId,
            "Finite-Strain Tetra Continuum",
            new Version(1, 0, 0),
            SimulationBackendCapabilities.TetrahedralContinuum
                | SimulationBackendCapabilities.ImplicitNonlinearTransient
                | SimulationBackendCapabilities.LargeStrain
                | SimulationBackendCapabilities.ElastoPlasticity
                | SimulationBackendCapabilities.AugmentedLagrangianContact
                | SimulationBackendCapabilities.MortarContact
                | SimulationBackendCapabilities.MonolithicThermoMechanical
                | SimulationBackendCapabilities.ScalableIterativeSolvers
                | SimulationBackendCapabilities.LargeResultStorage,
            "Updated-Lagrangian finite-strain continuum path with objective constitutive updates and advanced contact/coupling boundaries."
        );

    public BackendRunOutput Run(
        SimulationExecutionRequest request,
        CancellationToken ct = default
    ) => FiniteStrainBackendRunner.Run(request, ct);
}
