namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalMesherValidationPolicy(
    int MinimumElements = 1,
    double MinimumMeanRatio = 1e-4,
    bool RejectInverted = true
);

public sealed record ExternalMesherValidationResult(bool Passed, IReadOnlyList<string> Findings);

public static class ExternalMesherResultValidator
{
    public static ExternalMesherValidationResult Validate(
        TetrahedralMesh mesh,
        ExternalMesherValidationPolicy? policy = null
    )
    {
        policy ??= new();
        var findings = new List<string>();
        var quality = TetraQualityAnalyzer.Analyze(mesh);
        if (quality.ElementCount < policy.MinimumElements)
            findings.Add(
                $"Expected at least {policy.MinimumElements} tetrahedra; got {quality.ElementCount}."
            );
        if (policy.RejectInverted && quality.InvertedCount > 0)
            findings.Add($"Mesh contains {quality.InvertedCount} inverted tetrahedra.");
        if (quality.MinMeanRatio < policy.MinimumMeanRatio)
            findings.Add(
                $"Minimum mean-ratio {quality.MinMeanRatio:G6} is below {policy.MinimumMeanRatio:G6}."
            );
        return new(findings.Count == 0, findings);
    }
}
