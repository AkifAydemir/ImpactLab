namespace ImpactLab.Core.Meshing;

public sealed record MeshQualityReport(
    int NodeCount,
    int SpringCount,
    int PartCount,
    int SurfaceNodeCount,
    int IsolatedNodeCount,
    int MinimumDegree,
    int MaximumDegree,
    double AverageDegree,
    double MinimumNodeMassKg,
    double MaximumNodeMassKg,
    double MinimumSpringLengthMeters,
    double MaximumSpringLengthMeters,
    double SurfaceFraction
)
{
    public bool HasConnectivityWarnings => IsolatedNodeCount > 0 || MinimumDegree < 2;
}
