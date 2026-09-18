using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Analysis;

public static class ResultAnalyzer
{
    public static RunResultSummary Analyze(SimulationMesh mesh, SimulationResult result)
    {
        var last = result.Frames.Count == 0 ? null : result.Frames[^1];
        var parts = new List<PartResultSummary>();
        foreach (var part in mesh.Parts)
        {
            var broken = mesh
                .Springs.Skip(part.SpringStart)
                .Take(part.SpringCount)
                .Count(x => x.IsBroken);
            var maxDisplacement = 0.0;
            var maxDamage = 0.0;
            var residualKe = 0.0;
            if (last is not null)
            {
                for (var i = part.NodeStart; i < part.NodeEndExclusive; i++)
                {
                    maxDisplacement = Math.Max(
                        maxDisplacement,
                        (last.Position[i] - mesh.Nodes[i].RestPosition).Length
                    );
                    maxDamage = Math.Max(maxDamage, last.NodeDamage[i]);
                    residualKe += 0.5 * mesh.Nodes[i].MassKg * last.Velocity[i].LengthSquared;
                }
            }
            parts.Add(
                new PartResultSummary(
                    part.PartId,
                    part.NodeCount,
                    part.SpringCount,
                    broken,
                    part.SpringCount == 0 ? 0.0 : (double)broken / part.SpringCount,
                    maxDisplacement,
                    maxDamage,
                    residualKe
                )
            );
        }
        return new RunResultSummary(
            last?.TimeSeconds ?? 0.0,
            result.PeakKineticEnergyJ,
            result.PeakElasticEnergyJ,
            result.MaxDisplacementMeters,
            result.BrokenSpringCount,
            result.PeakContactForceN,
            result.PeakTangentialForceN,
            result.FrictionEnergyJ,
            parts
        );
    }
}
