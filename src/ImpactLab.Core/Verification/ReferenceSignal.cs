namespace ImpactLab.Core.Verification;

public readonly record struct ReferenceSignalPoint(double TimeSeconds, double Value);

public sealed record ReferenceSignal(
    string Id,
    string Name,
    string Unit,
    IReadOnlyList<ReferenceSignalPoint> Points
)
{
    public void Validate()
    {
        if (Points.Count < 2)
            throw new InvalidOperationException("Reference signal requires at least two points.");
        if (Points.Zip(Points.Skip(1)).Any(x => x.First.TimeSeconds >= x.Second.TimeSeconds))
            throw new InvalidOperationException(
                "Reference signal time must be strictly increasing."
            );
    }
}
