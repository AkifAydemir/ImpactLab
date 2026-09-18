namespace ImpactLab.Core.Calibration;

public sealed record CalibrationCandidateResult(
    CalibrationCandidate Candidate,
    double Score,
    string? Error = null
);

public sealed record CalibrationResult(
    string Name,
    IReadOnlyList<CalibrationCandidateResult> Candidates
)
{
    public CalibrationCandidateResult? Best =>
        Candidates.Where(x => x.Error is null).OrderBy(x => x.Score).FirstOrDefault();
}
