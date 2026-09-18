namespace ImpactLab.App.Visualization;

public sealed record ContourScaleSettings(
    ContourScaleMode Mode = ContourScaleMode.Global,
    double ManualMin = 0,
    double ManualMax = 1,
    double LowerPercentile = 0.02,
    double UpperPercentile = 0.98
);
