using ImpactLab.Core.Meshing;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Contact;

public sealed class PenaltyRigidContactModel : IRigidContactModel
{
    public ContactPairMetrics Apply(
        SimulationMesh mesh,
        SimulationState state,
        RigidBodyState body,
        MeshPartRange targetPart,
        IReadOnlyList<int> surfaceNodes,
        ContactSettings settings,
        ContactPairRule rule,
        double timeStepSeconds
    ) =>
        RigidContactSolver.Apply(
            mesh,
            state,
            body,
            targetPart,
            surfaceNodes,
            settings,
            rule,
            timeStepSeconds
        );
}
