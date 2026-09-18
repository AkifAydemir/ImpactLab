namespace ImpactLab.Core.Continuum.Contact;

public sealed record DynamicSurfaceContactSettings(
    double NormalPenaltyNPerM = 5e8,
    double NormalDampingNsPerM = 2e4,
    double StaticFriction = 0.30,
    double DynamicFriction = 0.22,
    double SlipRegularizationMps = 0.02,
    double SearchPaddingMeters = 0.002,
    bool EnableCcd = true
)
{
    public void Validate()
    {
        if (
            NormalPenaltyNPerM <= 0
            || NormalDampingNsPerM < 0
            || StaticFriction < 0
            || DynamicFriction < 0
            || SlipRegularizationMps <= 0
            || SearchPaddingMeters < 0
        )
            throw new InvalidOperationException("Invalid dynamic contact settings.");
    }
}
