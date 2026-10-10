using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Fracture;

public sealed class ConnectedLatticeFragmentTopologyModel : IFragmentTopologyModel
{
    public IReadOnlyList<FragmentClusterSnapshot> Analyze(
        SimulationMesh mesh,
        SimulationState state
    ) => FragmentClusterAnalyzer.Analyze(mesh, state);
}
