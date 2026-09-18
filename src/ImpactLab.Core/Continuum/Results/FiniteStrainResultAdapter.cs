using ImpactLab.Core.Constraints;
using ImpactLab.Core.Continuum.FiniteStrain;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Continuum.Results;

public static class FiniteStrainResultAdapter
{
    public static SimulationResult ToCompatibility(FiniteStrainRunResult result)
    {
        var rest = result.Mesh.Nodes.Select(n => n.Position).ToArray();
        var frames = result
            .Frames.Select(f => new SimulationFrame(
                f.TimeSeconds,
                f.Position.ToArray(),
                Enumerable.Repeat(Vec3.Zero, f.Position.Count).ToArray(),
                new double[f.Position.Count],
                0,
                0,
                f.InternalEnergyJ,
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
            .SelectMany(f => f.Position.Select((p, i) => (p - rest[i]).Length))
            .DefaultIfEmpty()
            .Max();
        return new SimulationResult(
            frames,
            new SimulationTelemetrySeries(),
            max,
            0,
            0,
            frames.Select(x => x.ElasticEnergyJ).DefaultIfEmpty().Max(),
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
