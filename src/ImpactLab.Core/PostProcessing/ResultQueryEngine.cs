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
        var derived = RequiresDerivedField(query.Field)
            ? LatticeDerivedFieldCalculator.Evaluate(mesh, frame)
            : null;
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
            var value = Value(query.Field, mesh, node.Id, frame, derived);
            if (query.MinimumValue is { } min && value < min)
                continue;
            if (query.MaximumValue is { } max && value > max)
                continue;
            rows.Add(
                new NodeResultRecord(
                    i,
                    node.PartId,
                    position,
                    value,
                    Unit(query.Field),
                    frame.NodeDamage[i]
                )
            );
        }
        return rows;
    }

    private static double Value(
        ResultFieldKind field,
        SimulationMesh mesh,
        int i,
        SimulationFrame frame,
        LatticeDerivedFields? derived
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
            ResultFieldKind.EquivalentStrain => derived!.EquivalentStrain[i],
            ResultFieldKind.EquivalentStress => derived!.EquivalentStressPa[i],
            ResultFieldKind.YieldSafetyFactor => derived!.YieldSafetyFactor[i],
            _ => 0.0,
        };

    public static string Unit(ResultFieldKind field) =>
        field switch
        {
            ResultFieldKind.DisplacementMagnitude
            or ResultFieldKind.PositionX
            or ResultFieldKind.PositionY
            or ResultFieldKind.PositionZ => "m",
            ResultFieldKind.Speed
            or ResultFieldKind.VelocityX
            or ResultFieldKind.VelocityY
            or ResultFieldKind.VelocityZ => "m/s",
            ResultFieldKind.EquivalentStress => "Pa",
            ResultFieldKind.Damage
            or ResultFieldKind.EquivalentStrain
            or ResultFieldKind.YieldSafetyFactor => "-",
            _ => "-",
        };

    private static bool RequiresDerivedField(ResultFieldKind field) =>
        field
            is ResultFieldKind.EquivalentStrain
                or ResultFieldKind.EquivalentStress
                or ResultFieldKind.YieldSafetyFactor;
}
