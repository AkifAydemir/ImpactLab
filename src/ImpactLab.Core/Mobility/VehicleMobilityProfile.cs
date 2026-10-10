namespace ImpactLab.Core.Mobility;

public sealed record VehicleMobilityProfile(
    string Id,
    string Name,
    double MassKg,
    double GroundContactAreaM2,
    double GroundClearanceMeters,
    double MaximumSlopeDegrees,
    double MaximumStepHeightMeters,
    double MaximumWaterDepthMeters,
    double TargetSpeedMetersPerSecond,
    double StaticDynamicLoadFactor = 1.15
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Vehicle identity is required.");
        if (MassKg <= 0 || GroundContactAreaM2 <= 0 || GroundClearanceMeters <= 0)
            throw new InvalidOperationException(
                "Vehicle mass, contact area and clearance must be positive."
            );
        if (MaximumSlopeDegrees is <= 0 or >= 90)
            throw new InvalidOperationException("Maximum slope must be between 0 and 90 degrees.");
        if (MaximumStepHeightMeters <= 0 || MaximumWaterDepthMeters < 0)
            throw new InvalidOperationException("Vehicle obstacle limits are invalid.");
        if (TargetSpeedMetersPerSecond < 0 || StaticDynamicLoadFactor < 1)
            throw new InvalidOperationException("Vehicle speed or dynamic load factor is invalid.");
    }
}
