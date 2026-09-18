using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public static class ConstraintReactionRecorder
{
    public static ConstraintReactionSample Measure(
        string constraintId,
        double timeSeconds,
        SimulationMesh mesh,
        IReadOnlyList<Vec3> velocityBeforeProjection,
        SimulationState state,
        double timeStepSeconds
    )
    {
        var impulse = Vec3.Zero;
        var corrected = 0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var dv = state.Velocity[i] - velocityBeforeProjection[i];
            if (dv.LengthSquared <= 1e-24)
                continue;
            impulse += dv * mesh.Nodes[i].MassKg;
            corrected++;
        }
        var force = timeStepSeconds <= 0.0 ? Vec3.Zero : impulse / timeStepSeconds;
        return new ConstraintReactionSample(
            constraintId,
            timeSeconds,
            force,
            force.Length,
            corrected
        );
    }
}
