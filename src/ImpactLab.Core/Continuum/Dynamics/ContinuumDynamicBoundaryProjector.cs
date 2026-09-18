using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Dynamics;

public sealed class ContinuumDynamicBoundaryProjector
{
    private readonly IReadOnlyList<ContinuumDirichletDof> _dofs;

    public ContinuumDynamicBoundaryProjector(IReadOnlyList<ContinuumDirichletDof> dofs) =>
        _dofs = dofs;

    public void Project(ContinuumDynamicState state)
    {
        foreach (var constrainedDof in _dofs)
        {
            var node = constrainedDof.Dof / 3;
            var axis = constrainedDof.Dof % 3;
            var displacement = state.Displacement[node];
            var velocity = state.Velocity[node];
            var acceleration = state.Acceleration[node];

            if (axis == 0)
            {
                displacement = new Vec3(constrainedDof.Value, displacement.Y, displacement.Z);
                velocity = new Vec3(0, velocity.Y, velocity.Z);
                acceleration = new Vec3(0, acceleration.Y, acceleration.Z);
            }
            else if (axis == 1)
            {
                displacement = new Vec3(displacement.X, constrainedDof.Value, displacement.Z);
                velocity = new Vec3(velocity.X, 0, velocity.Z);
                acceleration = new Vec3(acceleration.X, 0, acceleration.Z);
            }
            else
            {
                displacement = new Vec3(displacement.X, displacement.Y, constrainedDof.Value);
                velocity = new Vec3(velocity.X, velocity.Y, 0);
                acceleration = new Vec3(acceleration.X, acceleration.Y, 0);
            }

            state.Displacement[node] = displacement;
            state.Velocity[node] = velocity;
            state.Acceleration[node] = acceleration;
        }
    }
}
