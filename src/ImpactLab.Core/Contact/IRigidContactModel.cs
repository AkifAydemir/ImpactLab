using ImpactLab.Core.Meshing;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Contact;

public interface IRigidContactModel
{
    ContactPairMetrics Apply(
        SimulationMesh mesh,
        SimulationState state,
        RigidBodyState body,
        MeshPartRange targetPart,
        IReadOnlyList<int> surfaceNodes,
        ContactSettings settings,
        ContactPairRule rule,
        double timeStepSeconds
    );
}
