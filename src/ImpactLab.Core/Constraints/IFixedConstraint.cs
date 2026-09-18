using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public interface IFixedConstraint
{
    void Apply(SimulationMesh mesh, SimulationState state);
}
