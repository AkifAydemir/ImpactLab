namespace ImpactLab.Core.Runs;

public sealed record RunAnnotation(DateTimeOffset CreatedUtc, string Text, string? Author = null);
