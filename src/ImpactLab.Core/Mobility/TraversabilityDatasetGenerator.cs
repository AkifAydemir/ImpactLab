namespace ImpactLab.Core.Mobility;

public static class TraversabilityDatasetGenerator
{
    public static IReadOnlyList<TraversabilityDatasetRow> GenerateSynthetic(
        VehicleMobilityProfile vehicle,
        int count,
        int seed = 1729
    )
    {
        vehicle.Validate();
        if (count is < 1 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(count));
        var random = new Random(seed);
        var rows = new List<TraversabilityDatasetRow>(count);
        for (var i = 0; i < count; i++)
        {
            var terrain = new TerrainCellObservation(
                i / 1000,
                i % 1000,
                Next(random, -10, 100),
                Next(random, 0, Math.Min(60, vehicle.MaximumSlopeDegrees * 1.5)),
                Next(random, 0, vehicle.GroundClearanceMeters * 0.8),
                Next(random, 0, vehicle.MaximumStepHeightMeters * 1.5),
                LogUniform(random, 20_000, 5_000_000),
                Next(random, 0.15, 1.1),
                Next(random, 0, Math.Max(0.05, vehicle.MaximumWaterDepthMeters * 1.5)),
                random.NextDouble() < 0.25 ? vehicle.MassKg * 9.80665 * Next(random, 0.5, 4.0) : 0,
                Next(random, 0.5, 1.0)
            );
            rows.Add(new(vehicle, terrain, TraversabilityAssessor.Assess(vehicle, terrain)));
        }
        return rows;
    }

    private static double Next(Random random, double minimum, double maximum) =>
        minimum + random.NextDouble() * (maximum - minimum);

    private static double LogUniform(Random random, double minimum, double maximum) =>
        Math.Exp(Next(random, Math.Log(minimum), Math.Log(maximum)));
}
