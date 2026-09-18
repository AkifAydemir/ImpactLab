namespace ImpactLab.Core.Results.Expressions;

public sealed record FieldEvaluationContext(
    Func<string, double[]> ReadField,
    int ItemCount,
    int Components
);
