namespace ImpactLab.App.Visualization;

public static class ResultLodSelector
{
    public static ResultLevelOfDetail Select(int triangles, double frameBudgetMs) =>
        triangles switch
        {
            < 100000 => ResultLevelOfDetail.Full,
            < 300000 => frameBudgetMs >= 20 ? ResultLevelOfDetail.High : ResultLevelOfDetail.Medium,
            < 1000000 => ResultLevelOfDetail.Medium,
            _ => ResultLevelOfDetail.Low,
        };
}
