namespace ImpactLab.Core.Backends;

public sealed class AdvancedLatticeBackend : ISimulationBackend
{
    public const string BackendId = "advanced-lattice-v1";
    public SimulationBackendDescriptor Descriptor { get; } =
        new(
            BackendId,
            "Advanced Adaptive Lattice Dynamics",
            new Version(1, 1, 0),
            SimulationBackendCapabilities.DeformableDynamics
                | SimulationBackendCapabilities.RigidContact
                | SimulationBackendCapabilities.Damage
                | SimulationBackendCapabilities.KinematicBoundaries
                | SimulationBackendCapabilities.ImportedGeometry
                | SimulationBackendCapabilities.AdaptiveMeshing
                | SimulationBackendCapabilities.RateTemperatureMaterials
                | SimulationBackendCapabilities.CohesiveInterfaces
                | SimulationBackendCapabilities.PerformanceTracing,
            "Adaptive lattice backend with constitutive/cohesive execution."
        );

    public BackendRunOutput Run(
        SimulationExecutionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var result = new AdvancedLatticeSolver().Run(request, cancellationToken);
        return new(result, new LatticeResultDomain(request.Compiled.Mesh));
    }
}
