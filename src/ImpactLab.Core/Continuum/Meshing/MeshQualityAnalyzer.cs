namespace ImpactLab.Core.Continuum.Meshing;

public static class MeshQualityAnalyzer
{
    public static MeshQualitySummary Analyze(TetrahedralMesh m)
    {
        var q = m.Elements.Select(e => TetraQualityEvaluator.Evaluate(m, e)).ToArray();
        return new(
            q.Length,
            q.Count(x => x.Inverted),
            q.Min(x => x.MeanRatio),
            q.Average(x => x.MeanRatio),
            q.Min(x => x.MinDihedralDegrees),
            q.Max(x => x.MaxDihedralDegrees)
        );
    }
}
