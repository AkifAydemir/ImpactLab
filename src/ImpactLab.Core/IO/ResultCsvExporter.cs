using System.Globalization;
using System.Text;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.IO;

public static class ResultCsvExporter
{
    public static void Write(
        string path,
        SimulationMesh mesh,
        SimulationResult result,
        int frameIndex = -1
    )
    {
        if (result.Frames.Count == 0)
            throw new InvalidOperationException("Simulation has no frames to export.");
        var index = frameIndex < 0 ? result.Frames.Count - 1 : frameIndex;
        index = Math.Clamp(index, 0, result.Frames.Count - 1);
        var frame = result.Frames[index];
        var fields = LatticeDerivedFieldCalculator.Evaluate(mesh, frame);
        var text = new StringBuilder();
        text.AppendLine(
            "node_id,part_id,active,time_s,x_m,y_m,z_m,displacement_m,speed_m_per_s,damage,equivalent_strain,equivalent_stress_pa,yield_safety_factor,material_id"
        );
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var node = mesh.Nodes[i];
            var position = frame.Position[i];
            var displacement = (position - node.RestPosition).Length;
            text.Append(i)
                .Append(',')
                .Append(Quote(node.PartId))
                .Append(',')
                .Append(frame.NodeActive is null || frame.NodeActive[i] ? "true" : "false")
                .Append(',')
                .Append(Number(frame.TimeSeconds))
                .Append(',')
                .Append(Number(position.X))
                .Append(',')
                .Append(Number(position.Y))
                .Append(',')
                .Append(Number(position.Z))
                .Append(',')
                .Append(Number(displacement))
                .Append(',')
                .Append(Number(frame.Velocity[i].Length))
                .Append(',')
                .Append(Number(frame.NodeDamage[i]))
                .Append(',')
                .Append(Number(fields.EquivalentStrain[i]))
                .Append(',')
                .Append(Number(fields.EquivalentStressPa[i]))
                .Append(',')
                .Append(Number(fields.YieldSafetyFactor[i]))
                .Append(',')
                .Append(Quote(node.Material.Id))
                .AppendLine();
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
    }

    private static string Number(double value) =>
        double.IsPositiveInfinity(value)
            ? "inf"
            : value.ToString("R", CultureInfo.InvariantCulture);

    private static string Quote(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
