using ImpactLab.Core.Loads;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public sealed class PrescribedVelocityConstraint : IKinematicConstraint
{
    public PrescribedVelocityConstraint(
        string id,
        NodeSelection selection,
        Vec3 velocityMps,
        PiecewiseLinearCurve? scale = null
    )
    {
        Id = id;
        Selection = selection;
        VelocityMps = velocityMps;
        Scale = scale;
    }

    public string Id { get; }
    public NodeSelection Selection { get; }
    public Vec3 VelocityMps { get; }
    public PiecewiseLinearCurve? Scale { get; }

    public void Apply(
        double timeSeconds,
        SimulationMesh mesh,
        SimulationState state,
        KinematicConstraintPhase phase
    )
    {
        if (phase != KinematicConstraintPhase.PostIntegrate)
            return;
        var velocity = VelocityMps * (Scale?.Evaluate(timeSeconds) ?? 1.0);
        foreach (var id in Selection.NodeIds)
            state.Velocity[id] = velocity;
    }
}
