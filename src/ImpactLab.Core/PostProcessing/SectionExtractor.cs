using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public static class SectionExtractor
{
    public static SectionFrame Extract(
        SimulationMesh mesh,
        SimulationFrame frame,
        CutPlaneDefinition plane,
        bool useDeformedPosition = true
    )
    {
        plane.Validate();
        var n = plane.UnitNormal;
        var samples = new List<SectionNodeSample>();
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var p = useDeformedPosition ? frame.Position[i] : mesh.Nodes[i].RestPosition;
            if (Math.Abs(Vec3.Dot(p, n) - plane.Offset) > plane.HalfThicknessMeters)
                continue;
            samples.Add(
                new SectionNodeSample(
                    i,
                    mesh.Nodes[i].PartId,
                    p,
                    (frame.Position[i] - mesh.Nodes[i].RestPosition).Length,
                    frame.Velocity[i].Length,
                    frame.NodeDamage[i]
                )
            );
        }
        return new(plane, frame.TimeSeconds, samples);
    }
}
