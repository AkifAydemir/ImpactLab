namespace ImpactLab.Core.Continuum.Contact;

public sealed record SurfaceSelfContactSettings(
    bool Enabled = true,
    double SearchDistanceMeters = 0.002,
    double PenaltyNPerM = 5e7,
    double DampingNsPerM = 2e3,
    double Friction = 0.2,
    int MaxPairsPerTriangle = 32
)
{
    public void Validate()
    {
        if (
            SearchDistanceMeters <= 0
            || PenaltyNPerM <= 0
            || DampingNsPerM < 0
            || Friction < 0
            || MaxPairsPerTriangle < 1
        )
            throw new InvalidOperationException("Invalid self-contact settings.");
    }
}
