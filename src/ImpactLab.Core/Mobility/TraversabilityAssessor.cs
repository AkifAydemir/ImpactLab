namespace ImpactLab.Core.Mobility;

public static class TraversabilityAssessor
{
    private const double Gravity = 9.80665;

    public static TraversabilityAssessment Assess(
        VehicleMobilityProfile vehicle,
        TerrainCellObservation terrain
    )
    {
        vehicle.Validate();
        terrain.Validate();
        var roughnessDynamic =
            1.0
            + Math.Min(
                2.0,
                vehicle.TargetSpeedMetersPerSecond
                    * terrain.RoughnessMeters
                    / Math.Max(vehicle.GroundClearanceMeters, 1e-9)
            );
        var dynamicFactor = vehicle.StaticDynamicLoadFactor * roughnessDynamic;
        var weightN = vehicle.MassKg * Gravity;
        var groundPressure = weightN * dynamicFactor / vehicle.GroundContactAreaM2;
        var bearing = groundPressure / terrain.BearingStrengthPa;
        var slope = terrain.SlopeDegrees / vehicle.MaximumSlopeDegrees;
        var requiredFriction = Math.Tan(terrain.SlopeDegrees * Math.PI / 180.0);
        var friction = requiredFriction / terrain.SurfaceFrictionCoefficient;
        var obstacle = terrain.ObstacleHeightMeters / vehicle.MaximumStepHeightMeters;
        var water =
            vehicle.MaximumWaterDepthMeters <= 0
                ? terrain.WaterDepthMeters > 0
                    ? double.PositiveInfinity
                    : 0
                : terrain.WaterDepthMeters / vehicle.MaximumWaterDepthMeters;
        var structural =
            terrain.StructuralCapacityN <= 0
                ? 0
                : weightN * dynamicFactor / terrain.StructuralCapacityN;
        var factors = new[] { bearing, slope, friction, obstacle, water, structural };
        var worst = factors.Max();
        var status =
            worst > 1.0 ? TraversabilityStatus.Blocked
            : worst > 0.70 ? TraversabilityStatus.Caution
            : TraversabilityStatus.Passable;
        var reasons = Reasons(bearing, slope, friction, obstacle, water, structural);
        var finiteWorst = double.IsPositiveInfinity(worst) ? 10 : Math.Min(worst, 10);
        var cost =
            status == TraversabilityStatus.Blocked
                ? double.PositiveInfinity
                : 1.0 + 2.5 * finiteWorst * finiteWorst + 0.5 * (1.0 - terrain.DataConfidence);
        return new(
            status,
            cost,
            groundPressure,
            bearing,
            slope,
            friction,
            obstacle,
            water,
            structural,
            terrain.DataConfidence,
            reasons
        );
    }

    private static IReadOnlyList<string> Reasons(
        double bearing,
        double slope,
        double friction,
        double obstacle,
        double water,
        double structural
    )
    {
        var reasons = new List<string>();
        Add("Ground bearing capacity", bearing);
        Add("Slope limit", slope);
        Add("Available surface friction", friction);
        Add("Obstacle/step height", obstacle);
        Add("Water-depth limit", water);
        if (structural > 0)
            Add("Supporting structure capacity", structural);
        if (reasons.Count == 0)
            reasons.Add("All screening utilizations are below 70%.");
        return reasons;

        void Add(string name, double utilization)
        {
            if (utilization > 1)
                reasons.Add($"{name} exceeded ({utilization:P0}).");
            else if (utilization > 0.70)
                reasons.Add($"{name} is near the limit ({utilization:P0}).");
        }
    }
}
