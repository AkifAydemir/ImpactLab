namespace ImpactLab.Core.PostProcessing;

public sealed record ProbeSeries(ProbeDefinition Definition, IReadOnlyList<ProbeSample> Samples);
