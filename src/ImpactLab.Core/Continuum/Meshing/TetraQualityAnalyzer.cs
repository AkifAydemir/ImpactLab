namespace ImpactLab.Core.Continuum.Meshing;

public sealed record TetraMeshQualityReport(
    int ElementCount,
    int InvertedCount,
    double MinMeanRatio,
    double AverageMeanRatio,
    double MaxEdgeRatio
);

public static class TetraQualityAnalyzer
{
    public static TetraMeshQualityReport Analyze(TetrahedralMesh m)
    {
        var q = m.Elements.Select(e => TetraQualityEvaluator.Evaluate(m, e)).ToArray();
        return new(
            q.Length,
            q.Count(x => x.Inverted),
            q.Min(x => x.MeanRatio),
            q.Average(x => x.MeanRatio),
            q.Max(x => x.EdgeRatio)
        );
    }
}
