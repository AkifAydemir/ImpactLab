namespace ImpactLab.Core.Visualization;

public sealed record ChartSeries(
    string Name,
    string XUnit,
    string YUnit,
    IReadOnlyList<ChartPoint> Points,
    bool IsEnvelope = false
);
