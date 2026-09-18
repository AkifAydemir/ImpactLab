namespace ImpactLab.Core.Materials;

public sealed record MaterialInterfaceDefinition(
    string Id,
    string DisplayName,
    double StiffnessScale,
    double DampingScale,
    double YieldStrainScale,
    double FailureStrainScale
)
{
    public static MaterialInterfaceDefinition Neutral { get; } =
        new("neutral-interface", "Neutral interface", 1.0, 1.0, 1.0, 1.0);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Interface Id cannot be empty.");
        if (StiffnessScale <= 0.0)
            throw new InvalidOperationException("Interface stiffness scale must be positive.");
        if (DampingScale < 0.0)
            throw new InvalidOperationException("Interface damping scale cannot be negative.");
        if (YieldStrainScale <= 0.0 || FailureStrainScale <= 0.0)
            throw new InvalidOperationException("Interface strain scales must be positive.");
    }
}
