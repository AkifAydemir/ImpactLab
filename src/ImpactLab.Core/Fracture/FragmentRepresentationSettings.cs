namespace ImpactLab.Core.Fracture;

public sealed record FragmentRepresentationSettings
{
    public int MinimumResolvedNodeCount { get; init; } = 64;
    public int MinimumReducedOrderNodeCount { get; init; } = 4;
    public double MinimumResolvedMassKg { get; init; } = 0.1;
    public double MinimumReducedOrderMassKg { get; init; } = 0.001;

    public void Validate()
    {
        if (MinimumResolvedNodeCount < MinimumReducedOrderNodeCount)
            throw new InvalidOperationException(
                "Resolved fragment node threshold cannot be below the reduced-order threshold."
            );
        if (MinimumReducedOrderNodeCount < 1)
            throw new InvalidOperationException("Fragment node thresholds must be positive.");
        if (MinimumResolvedMassKg < MinimumReducedOrderMassKg)
            throw new InvalidOperationException(
                "Resolved fragment mass threshold cannot be below the reduced-order threshold."
            );
        if (MinimumReducedOrderMassKg < 0.0)
            throw new InvalidOperationException("Fragment mass thresholds cannot be negative.");
    }
}
