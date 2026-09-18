namespace ImpactLab.Core.PostProcessing;

public sealed record FrameExtrema(
    int FrameIndex,
    double TimeSeconds,
    double Minimum,
    double Maximum,
    double Mean,
    int NodeCount
);
