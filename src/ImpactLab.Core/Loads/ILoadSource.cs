using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public interface ILoadSource
{
    void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    );
}
