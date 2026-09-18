using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing;

public sealed class TetraSliverOptimizer
{
    public int Relax(TetrahedralMesh mesh, int passes = 3, double threshold = 0.08)
    {
        var boundaryNodes = TetrahedralSurfaceExtractor
            .Extract(mesh)
            .Triangles.SelectMany(triangle => triangle.Nodes())
            .ToHashSet();
        var moved = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            foreach (var element in mesh.Elements)
            {
                var quality = TetraQualityEvaluator.Evaluate(mesh, element);
                if (quality.MeanRatio >= threshold)
                    continue;

                var center =
                    element
                        .Nodes()
                        .Select(index => mesh.Nodes[index].Position)
                        .Aggregate(Vec3.Zero, (sum, position) => sum + position) / 4;
                foreach (var nodeId in element.Nodes())
                {
                    if (boundaryNodes.Contains(nodeId))
                        continue;

                    var node = mesh.Nodes[nodeId];
                    node.Position = Vec3.Lerp(node.Position, center, 0.08);
                    moved++;
                }
            }
        }

        return moved;
    }
}
