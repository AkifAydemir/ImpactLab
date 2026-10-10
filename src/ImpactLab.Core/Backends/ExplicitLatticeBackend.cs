using ImpactLab.Core.Contact;
using ImpactLab.Core.Fracture;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Physics;
using ImpactLab.Core.Scenarios;

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
        var physicsModels = request.Context?.PhysicsModels ?? PhysicsModelRegistry.CreateDefault();
        var damageModelId =
            request.SourceScenario?.Physics.DamageModelId
            ?? ScenarioPhysicsConfiguration.DefaultDamageModelId;
        var rigidContactModelId =
            request.SourceScenario?.Physics.RigidContactModelId
            ?? ScenarioPhysicsConfiguration.DefaultRigidContactModelId;
        var deformableContactModelId =
            request.SourceScenario?.Physics.DeformableContactModelId
            ?? ScenarioPhysicsConfiguration.DefaultDeformableContactModelId;
        var constitutiveModelId =
            request.SourceScenario?.Physics.LatticeConstitutiveModelId
            ?? ScenarioPhysicsConfiguration.DefaultLatticeConstitutiveModelId;
        var fragmentTopologyModelId =
            request.SourceScenario?.Physics.FragmentTopologyModelId
            ?? ScenarioPhysicsConfiguration.DefaultFragmentTopologyModelId;
        physicsModels.ResolvePlan(
            request
                .SourceScenario?.Physics.AdditionalModelIds.Prepend(deformableContactModelId)
                .Prepend(rigidContactModelId)
                .Prepend(constitutiveModelId)
                .Prepend(fragmentTopologyModelId)
                .Prepend(damageModelId)
                ??
                [
                    damageModelId,
                    fragmentTopologyModelId,
                    constitutiveModelId,
                    rigidContactModelId,
                    deformableContactModelId,
                ]
        );
        var result = new ExplicitLatticeSolver(
            physicsModels.Create<IDamageModel>(damageModelId),
            physicsModels.Create<IRigidContactModel>(rigidContactModelId),
            physicsModels.Create<IDeformableContactModelFactory>(deformableContactModelId),
            physicsModels.Create<ILatticeConstitutiveModel>(constitutiveModelId),
            physicsModels.Create<IFragmentTopologyModel>(fragmentTopologyModelId)
        ).Run(
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
