namespace ImpactLab.Core.Analysis;

public sealed record ContinuumResultSummary(
    int Nodes,
    int Elements,
    double MaxDisplacementMeters,
    double PeakVonMisesPa,
    double MaxTemperatureKelvin,
    double MinTemperatureKelvin,
    bool LinearConverged,
    int TotalLinearIterations
);
