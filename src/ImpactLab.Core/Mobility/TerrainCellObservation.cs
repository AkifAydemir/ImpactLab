namespace ImpactLab.Core.Mobility;

public sealed record TerrainCellObservation(
    int Row,
    int Column,
    double ElevationMeters,
    double SlopeDegrees,
    double RoughnessMeters,
    double ObstacleHeightMeters,
    double BearingStrengthPa,
    double SurfaceFrictionCoefficient,
    double WaterDepthMeters = 0,
    double StructuralCapacityN = 0,
    double DataConfidence = 1.0
)
{
    public void Validate()
    {
        if (Row < 0 || Column < 0)
            throw new InvalidOperationException("Terrain grid coordinates cannot be negative.");
        if (SlopeDegrees is < 0 or >= 90 || RoughnessMeters < 0 || ObstacleHeightMeters < 0)
            throw new InvalidOperationException("Terrain geometry values are invalid.");
        if (BearingStrengthPa <= 0 || SurfaceFrictionCoefficient <= 0)
            throw new InvalidOperationException("Terrain strength and friction must be positive.");
        if (WaterDepthMeters < 0 || StructuralCapacityN < 0 || DataConfidence is < 0 or > 1)
            throw new InvalidOperationException("Terrain capacity or confidence is invalid.");
    }
}
