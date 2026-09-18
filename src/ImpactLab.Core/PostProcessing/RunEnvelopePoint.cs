namespace ImpactLab.Core.PostProcessing;

public readonly record struct RunEnvelopePoint(
    double TimeSeconds,
    double Minimum,
    double Maximum,
    double Mean
);

public sealed record RunEnvelope(string Name, IReadOnlyList<RunEnvelopePoint> Points);
