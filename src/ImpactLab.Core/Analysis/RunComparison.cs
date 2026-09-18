namespace ImpactLab.Core.Analysis;

public sealed record RunComparison(
    string BaselineName,
    string CandidateName,
    double MaxDisplacementDeltaMeters,
    int BrokenSpringDelta,
    double PeakContactForceDeltaN,
    double FrictionEnergyDeltaJ
)
{
    public static RunComparison Compare(
        string baselineName,
        RunResultSummary baseline,
        string candidateName,
        RunResultSummary candidate
    ) =>
        new(
            baselineName,
            candidateName,
            candidate.MaxDisplacementMeters - baseline.MaxDisplacementMeters,
            candidate.BrokenSpringCount - baseline.BrokenSpringCount,
            candidate.PeakContactForceN - baseline.PeakContactForceN,
            candidate.FrictionEnergyJ - baseline.FrictionEnergyJ
        );
}
