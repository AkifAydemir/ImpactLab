using ImpactLab.Core.Constraints;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Continuum.Results;

public static class ContinuumResultAdapter
{
    public static SimulationResult ToCompatibility(ContinuumResult result)
    {
        var frames = result
            .Frames.Select(f => new SimulationFrame(
                f.TimeSeconds,
                f.Position,
                Enumerable.Repeat(Vec3.Zero, f.Position.Length).ToArray(),
                new double[f.Position.Length],
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
            ))
            .ToArray();
        var max = frames
            .SelectMany(f =>
                f.Position.Select((p, n) => (p - result.Mesh.Nodes[n].Position).Length)
            )
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
            result.ComputeTime,
            new ConstraintReactionSeries()
        );
    }
}
