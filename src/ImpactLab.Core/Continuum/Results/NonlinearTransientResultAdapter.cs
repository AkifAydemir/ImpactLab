using ImpactLab.Core.Constraints;
using ImpactLab.Core.Continuum.Nonlinear;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Continuum.Results;

public static class NonlinearTransientResultAdapter
{
    public static SimulationResult ToCompatibility(
        NonlinearTransientResult result,
        TimeSpan computeTime
    )
    {
        var rest = result.Mesh.Nodes.Select(n => n.Position).ToArray();
        var frames = result
            .Frames.Select(frame =>
            {
                var positions = new Vec3[rest.Length];
                for (var i = 0; i < positions.Length; i++)
                    positions[i] = rest[i] + frame.Displacement[i];
                return new SimulationFrame(
                    frame.TimeSeconds,
                    positions,
                    frame.Velocity.ToArray(),
                    new double[positions.Length],
                    0,
                    0,
                    0,
                    [],
                    [],
                    [],
                    0,
                    0,
                    0,
                    0,
                    0
                );
            })
            .ToArray();
        var max = result
            .Frames.SelectMany(f => f.Displacement.Select(d => d.Length))
            .DefaultIfEmpty()
            .Max();
        return new SimulationResult(
            frames,
            new SimulationTelemetrySeries(),
            max,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            computeTime,
            new ConstraintReactionSeries()
        );
    }
}
