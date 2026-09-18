using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public static class ResultQueryEngine
{
    public static IReadOnlyList<NodeResultRecord> Execute(
        SimulationMesh mesh,
        SimulationResult result,
        ResultQuery query
    )
    {
        if (result.Frames.Count == 0)
            return [];
        var frame = result.Frames[Math.Clamp(query.FrameIndex, 0, result.Frames.Count - 1)];
        var rows = new List<NodeResultRecord>();
        for (var i = 0; i < mesh.Nodes.Count && rows.Count < Math.Max(1, query.Limit); i++)
        {
            var node = mesh.Nodes[i];
            if (
                query.PartId is not null
                && !string.Equals(node.PartId, query.PartId, StringComparison.OrdinalIgnoreCase)
            )
                continue;
            var position = frame.Position[i];
            if (query.Bounds is { } bounds && !bounds.Contains(position))
                continue;
            var value = Value(query.Field, mesh, node.Id, frame);
            if (query.MinimumValue is { } min && value < min)
                continue;
            if (query.MaximumValue is { } max && value > max)
                continue;
            rows.Add(new NodeResultRecord(i, node.PartId, position, value, frame.NodeDamage[i]));
        }
        return rows;
    }

    private static double Value(
        ResultFieldKind field,
        SimulationMesh mesh,
        int i,
        SimulationFrame frame
    ) =>
        field switch
        {
            ResultFieldKind.DisplacementMagnitude => (
                frame.Position[i] - mesh.Nodes[i].RestPosition
            ).Length,
            ResultFieldKind.Speed => frame.Velocity[i].Length,
            ResultFieldKind.Damage => frame.NodeDamage[i],
            ResultFieldKind.PositionX => frame.Position[i].X,
            ResultFieldKind.PositionY => frame.Position[i].Y,
            ResultFieldKind.PositionZ => frame.Position[i].Z,
            ResultFieldKind.VelocityX => frame.Velocity[i].X,
            ResultFieldKind.VelocityY => frame.Velocity[i].Y,
            ResultFieldKind.VelocityZ => frame.Velocity[i].Z,
            _ => 0.0,
        };
}
