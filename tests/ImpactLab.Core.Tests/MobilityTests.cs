using ImpactLab.Core.Mobility;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class MobilityTests
{
    private static readonly VehicleMobilityProfile Vehicle = new(
        "utility",
        "Utility vehicle",
        3_500,
        0.8,
        0.35,
        35,
        0.35,
        0.6,
        5
    );

    [Fact]
    public void WeakBearingSurfaceIsBlocked()
    {
        var terrain = Cell(0, 0, bearingStrengthPa: 20_000);

        var result = TraversabilityAssessor.Assess(Vehicle, terrain);

        Assert.Equal(TraversabilityStatus.Blocked, result.Status);
        Assert.True(result.BearingUtilization > 1);
        Assert.Contains(
            result.Reasons,
            x => x.Contains("bearing", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Fact]
    public void RoutePlannerAvoidsBlockedCell()
    {
        var cells = new List<TerrainCellObservation>();
        for (var row = 0; row < 3; row++)
        for (var column = 0; column < 3; column++)
            cells.Add(
                Cell(row, column, bearingStrengthPa: row == 1 && column == 1 ? 10_000 : 2_000_000)
            );
        var map = new TerrainGridMap(3, 3, cells);

        var route = MobilityRoutePlanner.Plan(map, Vehicle, new(1, 0), new(1, 2));

        Assert.True(route.Found);
        Assert.DoesNotContain(new GridCoordinate(1, 1), route.Cells);
    }

    [Fact]
    public void TensorFlowDatasetIncludesSchemaAndLabels()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ImpactLab-mobility-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var rows = TraversabilityDatasetGenerator.GenerateSynthetic(Vehicle, 100, 42);
            var path = Path.Combine(root, "terrain.csv");

            TraversabilityDatasetExporter.Write(path, rows);

            Assert.Equal(100, File.ReadLines(path).Count() - 1);
            Assert.Contains("bearing_utilization", File.ReadLines(path).First());
            Assert.True(File.Exists(Path.ChangeExtension(path, ".schema.json")));
            Assert.Contains(
                "tensorflow",
                File.ReadAllText(Path.ChangeExtension(path, ".schema.json"))
            );
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static TerrainCellObservation Cell(int row, int column, double bearingStrengthPa) =>
        new(row, column, 0, 5, 0.01, 0.02, bearingStrengthPa, 0.7);
}
