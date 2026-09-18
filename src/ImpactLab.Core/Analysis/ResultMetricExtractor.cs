namespace ImpactLab.Core.Analysis;

public static class ResultMetricExtractor
{
    public static ResultMetricSet Extract(RunResultSummary summary)
    {
        var set = new ResultMetricSet();
        set.Set(ResultMetricKind.MaxDisplacementMeters, summary.MaxDisplacementMeters);
        set.Set(ResultMetricKind.BrokenSpringCount, summary.BrokenSpringCount);
        set.Set(ResultMetricKind.PeakContactForceN, summary.PeakContactForceN);
        set.Set(ResultMetricKind.PeakTangentialForceN, summary.PeakTangentialForceN);
        set.Set(ResultMetricKind.FrictionEnergyJ, summary.FrictionEnergyJ);
        set.Set(ResultMetricKind.PeakKineticEnergyJ, summary.PeakKineticEnergyJ);
        set.Set(ResultMetricKind.PeakElasticEnergyJ, summary.PeakElasticEnergyJ);
        return set;
    }
}
