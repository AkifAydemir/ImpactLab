namespace ImpactLab.Core.Verification.Benchmarks;

public sealed record VerificationBenchmarkMetric(
    string Id,
    string Unit,
    double Observed,
    double Reference,
    double AbsoluteTolerance,
    double RelativeTolerance
)
{
    public double AbsoluteError => Math.Abs(Observed - Reference);
    public double RelativeError => AbsoluteError / Math.Max(Math.Abs(Reference), 1e-15);
    public bool Passed => AbsoluteError <= AbsoluteTolerance || RelativeError <= RelativeTolerance;
}
