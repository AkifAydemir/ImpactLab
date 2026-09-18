using ImpactLab.Core.Physics;

namespace ImpactLab.Core.Backends;

public sealed class ExplicitLatticeBackend : ISimulationBackend
{
    public const string BackendId = "explicit-lattice-v1";
    public SimulationBackendDescriptor Descriptor { get; } =
        new(
            BackendId,
            "Explicit Lattice Dynamics",
            new Version(1, 1, 0),
            SimulationBackendCapabilities.DeformableDynamics
                | SimulationBackendCapabilities.RigidContact
                | SimulationBackendCapabilities.DeformableContact
                | SimulationBackendCapabilities.Damage
                | SimulationBackendCapabilities.KinematicBoundaries
                | SimulationBackendCapabilities.ReactionDiagnostics
                | SimulationBackendCapabilities.ImportedGeometry,
            "Legacy explicit spring/lattice dynamics backend retained as a fast engineering sandbox."
        );

    public BackendRunOutput Run(
        SimulationExecutionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var c = request.Compiled;
        var result = new ExplicitLatticeSolver(new MaterialResponseDamageModel()).Run(
            c.Mesh,
            request.Settings,
            c.Loads,
            c.Constraints,
            c.RigidBodies,
            request.ContactSettings,
            c.ContactInteractions,
            c.DeformableInteractions,
            cancellationToken,
            c.KinematicConstraints
        );
        return new(result, new LatticeResultDomain(c.Mesh));
    }
}
