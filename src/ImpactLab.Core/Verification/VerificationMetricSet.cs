namespace ImpactLab.Core.Verification;

public sealed record VerificationMetricSet(
    double Rmse,
    double Mae,
    double PeakAbsoluteError,
    double PeakRelativeError,
    double Correlation,
    int Samples
);
