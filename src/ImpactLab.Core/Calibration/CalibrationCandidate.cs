namespace ImpactLab.Core.Calibration;

public sealed record CalibrationCandidate(int Index, IReadOnlyDictionary<string, double> Values)
{
    public string Label =>
        $"Candidate {Index + 1}: " + string.Join(", ", Values.Select(x => $"{x.Key}={x.Value:G5}"));
}
