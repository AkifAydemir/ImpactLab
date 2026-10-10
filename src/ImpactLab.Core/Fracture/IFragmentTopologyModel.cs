using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Fracture;

public interface IFragmentTopologyModel
{
    IReadOnlyList<FragmentClusterSnapshot> Analyze(SimulationMesh mesh, SimulationState state);
}
