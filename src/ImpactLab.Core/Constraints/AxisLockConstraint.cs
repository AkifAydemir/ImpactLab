using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public sealed class AxisLockConstraint : IKinematicConstraint
{
    public AxisLockConstraint(string id, NodeSelection selection, AxisLockMask mask)
    {
        Id = id;
        Selection = selection;
        Mask = mask;
    }

    public string Id { get; }
    public NodeSelection Selection { get; }
    public AxisLockMask Mask { get; }

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
            var p = state.Position[id];
            var v = state.Velocity[id];
            state.Position[id] = new Vec3(
                Mask.HasFlag(AxisLockMask.X) ? rest.X : p.X,
                Mask.HasFlag(AxisLockMask.Y) ? rest.Y : p.Y,
                Mask.HasFlag(AxisLockMask.Z) ? rest.Z : p.Z
            );
            state.Velocity[id] = new Vec3(
                Mask.HasFlag(AxisLockMask.X) ? 0 : v.X,
                Mask.HasFlag(AxisLockMask.Y) ? 0 : v.Y,
                Mask.HasFlag(AxisLockMask.Z) ? 0 : v.Z
            );
        }
    }
}
