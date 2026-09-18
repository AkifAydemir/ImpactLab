using ImpactLab.Core.Loads;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public sealed class PrescribedDisplacementConstraint : IKinematicConstraint
{
    public PrescribedDisplacementConstraint(
        string id,
        NodeSelection selection,
        Vec3 direction,
        PiecewiseLinearCurve displacementMeters
    )
    {
        Id = id;
        Selection = selection;
        Direction = direction.Normalized();
        DisplacementMeters = displacementMeters;
    }

    public string Id { get; }
    public NodeSelection Selection { get; }
    public Vec3 Direction { get; }
    public PiecewiseLinearCurve DisplacementMeters { get; }

    public void Apply(
        double timeSeconds,
        SimulationMesh mesh,
        SimulationState state,
        KinematicConstraintPhase phase
    )
    {
        if (phase != KinematicConstraintPhase.PostIntegrate)
            return;
        var target = Direction * DisplacementMeters.Evaluate(timeSeconds);
        foreach (var id in Selection.NodeIds)
        {
            state.Position[id] = mesh.Nodes[id].RestPosition + target;
            state.Velocity[id] = Vec3.Zero;
        }
    }
}
