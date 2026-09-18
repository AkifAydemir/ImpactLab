using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public sealed class SymmetryPlaneConstraint : IKinematicConstraint
{
    public SymmetryPlaneConstraint(string id, NodeSelection selection, Vec3 normal)
    {
        Id = id;
        Selection = selection;
        Normal = normal.Normalized();
    }

    public string Id { get; }
    public NodeSelection Selection { get; }
    public Vec3 Normal { get; }

    public void Apply(
        double timeSeconds,
        SimulationMesh mesh,
        SimulationState state,
        KinematicConstraintPhase phase
    )
    {
        foreach (var id in Selection.NodeIds)
        {
            var rest = mesh.Nodes[id].RestPosition;
            var delta = state.Position[id] - rest;
            state.Position[id] -= Normal * Vec3.Dot(delta, Normal);
            state.Velocity[id] -= Normal * Vec3.Dot(state.Velocity[id], Normal);
        }
    }
}
