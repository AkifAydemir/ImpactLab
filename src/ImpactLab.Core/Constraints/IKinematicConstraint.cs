using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public interface IKinematicConstraint
{
    string Id { get; }
    void Apply(
        double timeSeconds,
        SimulationMesh mesh,
        SimulationState state,
        KinematicConstraintPhase phase
    );
}
