using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Meshing;

public static class RefinementPlanner
{
    public static RefinementRecommendation FromDamage(
        SimulationMesh mesh,
        SimulationFrame frame,
        int maxRegions = 12,
        double threshold = 0.35
    )
    {
        var candidates = Enumerable
            .Range(0, mesh.Nodes.Count)
            .Where(i => frame.NodeDamage[i] >= threshold)
            .OrderByDescending(i => frame.NodeDamage[i])
            .Take(Math.Max(1, maxRegions))
            .ToArray();
        var regions = new List<LocalMeshSizingRegion>();
        var half = mesh.CellSize * 2.0;
        for (var n = 0; n < candidates.Length; n++)
        {
            var p = frame.Position[candidates[n]];
            var d = new Vec3(half, half, half);
            regions.Add(
                new LocalMeshSizingRegion(
                    $"damage-refine-{n + 1}",
                    new BoundingBox3(p - d, p + d),
                    Math.Max(mesh.CellSize * 0.5, mesh.CellSize / 3.0),
                    100 - n
                )
            );
        }
        return new(
            regions,
            mesh.CellSize,
            regions.Count == 0
                ? "No region exceeded the damage refinement threshold."
                : "Local refinement recommended around high-damage gradients from the selected result frame."
        );
    }
}
