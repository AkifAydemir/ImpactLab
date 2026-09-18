namespace ImpactLab.Core.Continuum;

public sealed record StructuredTetraMesherSettings(
    double EdgeLengthMeters = 0.015,
    int MaxCellsPerAxis = 160
)
{
    public void Validate()
    {
        if (EdgeLengthMeters <= 0)
            throw new InvalidOperationException("Edge length must be positive.");
        if (MaxCellsPerAxis < 2)
            throw new InvalidOperationException("MaxCellsPerAxis must be >= 2.");
    }
}
