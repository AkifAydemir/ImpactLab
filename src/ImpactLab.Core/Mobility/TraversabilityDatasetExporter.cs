using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ImpactLab.Core.Mobility;

public static class TraversabilityDatasetExporter
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private static readonly string[] Columns =
    [
        "vehicle_mass_kg",
        "contact_area_m2",
        "ground_clearance_m",
        "max_slope_deg",
        "max_step_m",
        "max_water_depth_m",
        "target_speed_mps",
        "terrain_elevation_m",
        "terrain_slope_deg",
        "terrain_roughness_m",
        "obstacle_height_m",
        "bearing_strength_pa",
        "friction_coefficient",
        "water_depth_m",
        "structural_capacity_n",
        "data_confidence",
        "ground_pressure_pa",
        "bearing_utilization",
        "slope_utilization",
        "friction_utilization",
        "obstacle_utilization",
        "water_utilization",
        "structural_utilization",
        "cost_multiplier",
        "label",
        "label_name",
        "label_source",
    ];

    public static void Write(string csvPath, IReadOnlyCollection<TraversabilityDatasetRow> rows)
    {
        if (rows.Count == 0)
            throw new ArgumentException("At least one dataset row is required.", nameof(rows));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(csvPath))!);
        var text = new StringBuilder();
        text.AppendLine(string.Join(',', Columns));
        foreach (var row in rows)
        {
            var v = row.Vehicle;
            var t = row.Terrain;
            var a = row.Assessment;
            text.AppendLine(
                string.Join(
                    ',',
                    new[]
                    {
                        N(v.MassKg),
                        N(v.GroundContactAreaM2),
                        N(v.GroundClearanceMeters),
                        N(v.MaximumSlopeDegrees),
                        N(v.MaximumStepHeightMeters),
                        N(v.MaximumWaterDepthMeters),
                        N(v.TargetSpeedMetersPerSecond),
                        N(t.ElevationMeters),
                        N(t.SlopeDegrees),
                        N(t.RoughnessMeters),
                        N(t.ObstacleHeightMeters),
                        N(t.BearingStrengthPa),
                        N(t.SurfaceFrictionCoefficient),
                        N(t.WaterDepthMeters),
                        N(t.StructuralCapacityN),
                        N(t.DataConfidence),
                        N(a.GroundPressurePa),
                        N(a.BearingUtilization),
                        N(a.SlopeUtilization),
                        N(a.FrictionUtilization),
                        N(a.ObstacleUtilization),
                        N(a.WaterUtilization),
                        N(a.StructuralUtilization),
                        N(a.CostMultiplier),
                        ((int)a.Status).ToString(CultureInfo.InvariantCulture),
                        Q(a.Status.ToString()),
                        Q(row.LabelSource),
                    }
                )
            );
        }
        File.WriteAllText(csvPath, text.ToString(), new UTF8Encoding(false));
        WriteSchema(Path.ChangeExtension(csvPath, ".schema.json"));
    }

    private static void WriteSchema(string path)
    {
        var schema = new
        {
            schemaVersion = 1,
            format = "ImpactLab.TraversabilityDataset",
            columns = Columns,
            labelColumn = "label",
            labels = new Dictionary<int, string>
            {
                [(int)TraversabilityStatus.Passable] = "Passable",
                [(int)TraversabilityStatus.Caution] = "Caution",
                [(int)TraversabilityStatus.Blocked] = "Blocked",
            },
            tensorflow = new
            {
                loader = "tf.data.experimental.make_csv_dataset",
                batchSize = 32,
                labelName = "label",
            },
            warning = "Synthetic labels reproduce the ImpactLab screening rule; they are not field ground truth. Replace or augment them with measured vehicle/terrain outcomes before deployment.",
        };
        File.WriteAllText(path, JsonSerializer.Serialize(schema, Json), new UTF8Encoding(false));
    }

    private static string N(double value) =>
        double.IsPositiveInfinity(value)
            ? "1e308"
            : value.ToString("R", CultureInfo.InvariantCulture);

    private static string Q(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
