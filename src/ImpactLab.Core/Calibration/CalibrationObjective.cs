namespace ImpactLab.Core.Calibration;

public sealed record CalibrationObjective(
    string Name,
    double Weight,
    Func<CalibrationObservation, double> Error
)
{
    public double Evaluate(CalibrationObservation observation) =>
        Math.Max(0.0, Weight) * Math.Max(0.0, Error(observation));
}

public sealed record CalibrationObservation(
    IReadOnlyDictionary<string, double> Simulated,
    IReadOnlyDictionary<string, double> Reference
);
