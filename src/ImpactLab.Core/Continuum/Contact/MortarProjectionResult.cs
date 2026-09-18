namespace ImpactLab.Core.Continuum.Contact;

public sealed record MortarProjectionResult(
    int SlaveTriangleId,
    int MasterTriangleId,
    double Xi,
    double Eta,
    double GapMeters,
    double Weight,
    bool InsideMaster
);
