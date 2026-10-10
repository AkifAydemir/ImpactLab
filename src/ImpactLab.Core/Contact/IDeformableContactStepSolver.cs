using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Contact;

public interface IDeformableContactStepSolver
{
    DeformableContactMetrics Apply(
        SimulationMesh mesh,
        SimulationState state,
        IReadOnlyList<int> surfaceA,
        IReadOnlyList<int> surfaceB,
        string partA,
        string partB,
        ContactSettings settings,
        DeformableContactRule rule,
        double timeStepSeconds = 0.0
    );
}
