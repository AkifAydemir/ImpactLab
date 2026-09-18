namespace ImpactLab.Core.Contact;

public sealed record ContactSettings
{
    public static ContactSettings Default { get; } = new();
    public double NormalStiffnessNPerM { get; init; } = 2.0e7;
    public double NormalDampingNsPerM { get; init; } = 2.0e3;
    public double TangentialDampingNsPerM { get; init; } = 5.0e2;
    public double StaticFrictionCoefficient { get; init; } = 0.35;
    public double DynamicFrictionCoefficient { get; init; } = 0.25;
    public double TangentialSlipSpeedMPerSec { get; init; } = 0.03;
    public double NodeRadiusScale { get; init; } = 0.48;
    public double BroadPhasePaddingScale { get; init; } = 1.25;

    public void Validate()
    {
        if (NormalStiffnessNPerM <= 0.0)
            throw new InvalidOperationException("Contact stiffness must be positive.");
        if (NormalDampingNsPerM < 0.0 || TangentialDampingNsPerM < 0.0)
            throw new InvalidOperationException("Contact damping cannot be negative.");
        if (StaticFrictionCoefficient < 0.0 || DynamicFrictionCoefficient < 0.0)
            throw new InvalidOperationException("Friction coefficients cannot be negative.");
        if (DynamicFrictionCoefficient > StaticFrictionCoefficient)
            throw new InvalidOperationException(
                "Dynamic friction should not exceed static friction."
            );
        if (TangentialSlipSpeedMPerSec <= 0.0)
            throw new InvalidOperationException("Slip speed must be positive.");
        if (NodeRadiusScale is <= 0.0 or > 1.0)
            throw new InvalidOperationException("Node radius scale must be in (0, 1].");
        if (BroadPhasePaddingScale < 1.0)
            throw new InvalidOperationException("Broad-phase padding scale must be >= 1.");
    }
}
